using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Gears.Interpreter.Adapters;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public interface IElementSearchStrategy
    {
        IBufferedElement FindInput(string text, SearchDirection direction);
    }

    public class LocationHeuristictSearchStrategy : IElementSearchStrategy
    {
        private readonly ISeleniumAdapter _seleniumAdapter;

        public LocationHeuristictSearchStrategy(ISeleniumAdapter seleniumAdapter)
        {
            _seleniumAdapter = seleniumAdapter;
        }

        public IBufferedElement FindInput(string text, SearchDirection direction)
        {
            var theInput = default(IBufferedElement);

            var inputTagNames = new[] {"input", "textArea"};

            var labelsWithSearchedText = _seleniumAdapter.WebDriver.GetElementsByText(text);
            
            var allInputs = _seleniumAdapter.WebDriver.GetElementsByTagNames(inputTagNames);

            foreach (var label in labelsWithSearchedText)
            {
                if (inputTagNames.Contains(label.TagName.ToLower()))
                {
                    theInput = new BufferedElement(label);
                    break;
                }

                var domNeighbours = _seleniumAdapter.WebDriver.FilterDomNeighbours(allInputs, label);
                if (domNeighbours.Any())
                {
                    if (domNeighbours.Count() == 1)
                    {
                        theInput = new BufferedElement(domNeighbours.First());
                        break;
                    }
                }

                var orthogonalInputs = _seleniumAdapter.WebDriver.FilterOrthogonalElements(allInputs, label);

                var bufferedElements = _seleniumAdapter.WebDriver.SelectWithLocation(orthogonalInputs);

                bufferedElements = SortByDistance(bufferedElements, label.Location.X, label.Location.Y);

                bufferedElements = PutNeighborsToFront(bufferedElements, domNeighbours);


                if (direction == SearchDirection.Left)
                {
                    bufferedElements = FilterLeftItems(label.Location.X, bufferedElements);
                }

                if (bufferedElements.Any())
                {
                    theInput = bufferedElements.First();
                    break;
                }

                //Show.HighlightElements(bufferedElements.Select(x=>x.WebElement), Selenium);
            }
            return theInput;
        }

        private IEnumerable<IBufferedElement> FilterLeftItems(int x, IEnumerable<IBufferedElement> bufferedElements)
        {
            return bufferedElements.Where(e => e.Rectangle.Right < x);

        }

        

        private IEnumerable<IBufferedElement> PutNeighborsToFront(IEnumerable<IBufferedElement> bufferedElements, ReadOnlyCollection<IWebElement> domNeighbours)
        {
            var neighbors = new List<IBufferedElement>();
            var nonNeighbors = new List<IBufferedElement>();

            foreach (var bufferedElement in bufferedElements)
            {
                if (domNeighbours.Contains(bufferedElement.WebElement))
                {
                    neighbors.Add(bufferedElement);
                }
                else
                {
                    nonNeighbors.Add(bufferedElement);
                }
            }

            var result = new List<IBufferedElement>(neighbors);
            result.AddRange(nonNeighbors);

            return result;
        }

        private object GetDistance(int sourceX, int sourceY, int targetX, int targetY)
        {
            var x = sourceX - targetX;
            var y = sourceY - targetY;
            return (x * x + y * y);
        }

        private IEnumerable<IBufferedElement> SortByDistance(IEnumerable<IBufferedElement> bufferedElements, int x, int y)
        {
            return bufferedElements.OrderBy(e => GetDistance(e.Rectangle.Left, e.Rectangle.Top, x, y));
        }

    }

    
}