using System;
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
        IBufferedElement FindElementNextToAnotherElement(string text, SearchDirection direction);
        IElementSearchStrategy Elements(IEnumerable<string> searchedTagNames);
        IElementSearchStrategy WithText(string text);
        IElementSearchStrategy RelativeTo(string text, SearchDirection specDirection);
        IElementSearchStrategy SortBy(SearchDirection specDirection);
        IEnumerable<IBufferedElement> Results();
    }

    public class LocationHeuristictSearchStrategy : IElementSearchStrategy
    {
        private readonly ISeleniumAdapter _seleniumAdapter;

        public LocationHeuristictSearchStrategy(ISeleniumAdapter seleniumAdapter)
        {
            _seleniumAdapter = seleniumAdapter;
        }

        /// <summary>
        /// simple Fluent
        /// </summary>
        public ReadOnlyCollection<IWebElement> Query { get; set; }

        public IElementSearchStrategy Elements(IEnumerable<string> searchedTagNames)
        {
            if (searchedTagNames.Any())
            {
                Query = _seleniumAdapter.WebDriver.GetElementsByTagNames(searchedTagNames);
            }
            else
            {
                Query = _seleniumAdapter.WebDriver.GetAllElements();
            }
            
            return this;
        }

        public IElementSearchStrategy WithText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Query = _seleniumAdapter.WebDriver.FilterElementsByText(text, Query);
            }

            return this;
        }

        public IElementSearchStrategy RelativeTo(string visibleTextOfTheRelativeElement, SearchDirection direction)
        {
            if (string.IsNullOrEmpty(visibleTextOfTheRelativeElement))
            {
                return this;
            }

            var results = new List<IBufferedElement>();
            var relativeElements = _seleniumAdapter.WebDriver.FilterElementsByText(visibleTextOfTheRelativeElement, _seleniumAdapter.WebDriver.GetAllElements());

            var candidates = Query;

            foreach (var relative in relativeElements)
            {
                if (direction == SearchDirection.RightFromAnotherElementInclusive && 
                    Query.Any(q=>q.Equals(relative)))
                {
                    results.AddRange(Query.Where(q => q.Equals(relative)).Select(q=>new BufferedElement(q)));
                }

                var domNeighbours = _seleniumAdapter.WebDriver.FilterDomNeighbours(candidates, relative);
                //if (domNeighbours.Any())
                //{
                //    if (domNeighbours.Count() == 1)
                //    {
                //        results.Add(new BufferedElement(domNeighbours.First()));
                //    }
                //}

                var orthogonalInputs = _seleniumAdapter.WebDriver.FilterOrthogonalElements(candidates, relative);

                var bufferedElements = _seleniumAdapter.WebDriver.SelectWithLocation(orthogonalInputs);

                bufferedElements = SortByDistance(bufferedElements, relative.Location.X, relative.Location.Y);

                bufferedElements = PutNeighborsToFront(bufferedElements, domNeighbours);

                bufferedElements = FilterOtherDirections(direction, bufferedElements, relative);
                
                results.AddRange(bufferedElements);

                //Show.HighlightElements(bufferedElements.Select(x=>x.WebElement), Selenium);
            }

            Query = new ReadOnlyCollection<IWebElement>(results.Select(x=>x.WebElement).ToList());

            return this;
        }

        private static IEnumerable<IBufferedElement> FilterOtherDirections(SearchDirection direction, IEnumerable<IBufferedElement> bufferedElements,
            IWebElement relative)
        {
            switch (direction)
            {
                case SearchDirection.Up:
                    bufferedElements = bufferedElements.Where(e => e.Rectangle.Bottom < relative.Location.Y);
                    break;
                case SearchDirection.Down:
                    bufferedElements = bufferedElements.Where(e => e.Rectangle.Top > relative.Location.Y);
                    break;
                case SearchDirection.LeftFromAnotherElement:
                    bufferedElements = bufferedElements.Where(e => e.Rectangle.Right < relative.Location.X);
                    break;
                case SearchDirection.RightFromAnotherElement:
                case SearchDirection.RightFromAnotherElementInclusive:
                    bufferedElements = bufferedElements.Where(e => e.Rectangle.Left > relative.Location.X);
                    break;
            }
            return bufferedElements;
        }

        public IElementSearchStrategy SortBy(SearchDirection specDirection)
        {
            var buffs = Query.Select(x => x.AsBufferedElement());
            IOrderedEnumerable<IBufferedElement> orderedEnumerable;
            switch (specDirection)
            {
                case SearchDirection.Up:
                case SearchDirection.UpFromBottomEdge:
                    orderedEnumerable = buffs.OrderBy(e => e.Rectangle.Location.Y);
                    break;
                case SearchDirection.Down:
                case SearchDirection.DownFromTopEdge:
                    orderedEnumerable = buffs.OrderBy(e => e.Rectangle.Location.Y);
                    break;
                case SearchDirection.LeftFromRightEdge:
                case SearchDirection.LeftFromAnotherElement:
                    orderedEnumerable = buffs.OrderByDescending(e => e.Rectangle.Location.X);
                    break;
                default:
                    orderedEnumerable = buffs.OrderBy(e => e.Rectangle.Location.X);
                    break;
            }

            Query = new ReadOnlyCollection<IWebElement>(orderedEnumerable.Select(x => x.WebElement).ToList());

            return this;
        }

        public IEnumerable<IBufferedElement> Results()
        {
            return Query.Select(x=>x.AsBufferedElement());
        }

        public IBufferedElement FindElementNextToAnotherElement(string visibleTextOfTheRelativeElement, SearchDirection direction)
        {
            var returnValue = default(IBufferedElement);

            var searchedTagNames = new[] {"input", "textArea"};

            var relativeElements = _seleniumAdapter.WebDriver.FilterElementsByText(visibleTextOfTheRelativeElement, _seleniumAdapter.WebDriver.GetAllElements());
            
            var candidates = _seleniumAdapter.WebDriver.GetElementsByTagNames(searchedTagNames);

            foreach (var relative in relativeElements)
            {
                // TODO this will likely have to be done with another strategy
                if (RelativeHasTagName(searchedTagNames, relative))
                {
                    returnValue = new BufferedElement(relative);
                    break;
                }

                var domNeighbours = _seleniumAdapter.WebDriver.FilterDomNeighbours(candidates, relative);
                if (domNeighbours.Any())
                {
                    if (domNeighbours.Count() == 1)
                    {
                        returnValue = new BufferedElement(domNeighbours.First());
                        break;
                    }
                }

                var orthogonalInputs = _seleniumAdapter.WebDriver.FilterOrthogonalElements(candidates, relative);

                var bufferedElements = _seleniumAdapter.WebDriver.SelectWithLocation(orthogonalInputs);

                bufferedElements = SortByDistance(bufferedElements, relative.Location.X, relative.Location.Y);

                bufferedElements = PutNeighborsToFront(bufferedElements, domNeighbours);


                if (direction == SearchDirection.LeftFromAnotherElement)
                {
                    bufferedElements = bufferedElements.Where(e => e.Rectangle.Right < relative.Location.X);
                }

                if (bufferedElements.Any())
                {
                    returnValue = bufferedElements.First();
                    break;
                }

                //Show.HighlightElements(bufferedElements.Select(x=>x.WebElement), Selenium);
            }
            return returnValue;
        }

        private static bool RelativeHasTagName(string[] searchedTagNames, IWebElement relative)
        {
            return searchedTagNames.Contains(relative.TagName.ToLower());
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