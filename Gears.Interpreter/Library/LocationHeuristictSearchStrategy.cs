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
        IBufferedElement FindElementNextToAnotherElement(string text, SearchDirection direction, bool orthogonalOnly);

        LookupResult DirectLookup(List<string> searchedTagNames, string subjectName, string locale, SearchDirection searchDirection, int Order, bool orthogonalOnly);

        IElementSearchStrategy Elements(IEnumerable<string> searchedTagNames);
        IElementSearchStrategy WithText(string text, bool matchWhenTextIsInChild);
        IElementSearchStrategy RelativeTo(string text, SearchDirection specDirection, bool orthogonalOnly);
        IElementSearchStrategy SortBy(SearchDirection specDirection);
        IEnumerable<IBufferedElement> Results();
        bool Any();
        IElementSearchStrategy Elements();

        LookupResult DirectLookupWithNeighbours(string labelText, SearchDirection searchDirection, int order);
    }

    public class LookupResult
    {
        public LookupResult(bool success)
        {
            Success = success;
        }

        public LookupResult(List<IBufferedElement> otherValidResults, IBufferedElement result, bool success)
        {
            OtherValidResults = otherValidResults;
            Result = result;
            Success = success;
        }

        public List<IBufferedElement> OtherValidResults { get; set; }
        public IBufferedElement Result { get; set; }
        public bool Success { get; set; }
    }

    public class LocationHeuristictSearchStrategy : IElementSearchStrategy
    {
        private readonly ISeleniumAdapter _seleniumAdapter;

        private readonly ReadOnlyCollection<IWebElement> _query;

        public LocationHeuristictSearchStrategy(ISeleniumAdapter seleniumAdapter)
        {
            _seleniumAdapter = seleniumAdapter;
        }

        private LocationHeuristictSearchStrategy(ISeleniumAdapter seleniumAdapter, ReadOnlyCollection<IWebElement> query)
        {
            _seleniumAdapter = seleniumAdapter;
            _query = new ReadOnlyCollection<IWebElement>(new List<IWebElement>(query));
        }

        
        public LookupResult DirectLookup(List<string> searchedTagNames, string subjectName, string locale, SearchDirection searchDirection, int order, bool orthogonalOnly)
        {
            var lookingForSpecificElements = searchedTagNames.Any();

            var query = Elements(searchedTagNames).WithText(subjectName, lookingForSpecificElements);

            if (!string.IsNullOrEmpty(locale))
            {
                query = query.RelativeTo(locale, searchDirection, orthogonalOnly);
            }

            query = query.SortBy(searchDirection);

            var validResults = query.Results().ToList();

            if (order >= validResults.Count())
            {
                return new LookupResult(validResults, result: null, success: false);
            }

            return new LookupResult(validResults, validResults.Skip(order).First(), success: true);
        }

        /// <summary>
        /// This lookup method searches automatically for elements orthogonal to the specified text. If searching by instruction 'login' for instance, this method will also look for all elements next to all 'login' labels.
        /// </summary>
        /// <param name="labelText"></param>
        /// <param name="searchDirection"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public LookupResult DirectLookupWithNeighbours(string labelText, SearchDirection searchDirection, int order)
        {
            List<IBufferedElement> validResults;
            var allInputs = Elements(new[] { "input", "textArea" });

            var allInputsWithText = allInputs.WithText(labelText, matchWhenTextIsInChild: false);

            if (allInputsWithText.Any())
            {
                validResults = allInputsWithText
                    .SortBy(searchDirection)
                    .Results().ToList();
            }
            else
            {
                validResults = allInputs
                    .RelativeTo(labelText, searchDirection, true)
                    .SortBy(searchDirection)
                    .Results().ToList();
            }

            if (order >= validResults.Count())
            {
                return new LookupResult(validResults, result: null, success: false);
            }

            return new LookupResult(validResults, validResults.Skip(order).First(), success: true);
        }

        public IElementSearchStrategy Elements()
        {
            return Elements(new List<string>());
        }

        public IElementSearchStrategy Elements(IEnumerable<string> searchedTagNames)
        {
            var newQuery = _query;

            if (searchedTagNames.Any())
            {
                newQuery = _seleniumAdapter.WebDriver.GetElementsByTagNames(searchedTagNames);
            }
            else
            {
                newQuery = _seleniumAdapter.WebDriver.GetAllElements();
            }
            
            return new LocationHeuristictSearchStrategy(_seleniumAdapter, newQuery);
        }

        public IElementSearchStrategy WithText(string text, bool matchWhenTextIsInChild)
        {
            var newQuery = _query;

            if (!string.IsNullOrEmpty(text))
            {
                newQuery = _seleniumAdapter.WebDriver.FilterElementsByText(text, _query, matchWhenTextIsInChild);
            }

            return new LocationHeuristictSearchStrategy(_seleniumAdapter, newQuery);
        }

        public IElementSearchStrategy RelativeTo(string visibleTextOfTheRelativeElement, SearchDirection direction, bool orthogonalOnly)
        {
            if (string.IsNullOrEmpty(visibleTextOfTheRelativeElement))
            {
                return new LocationHeuristictSearchStrategy(_seleniumAdapter, _query);
            }

            var results = new List<IBufferedElement>();
            var relativeElements = _seleniumAdapter.WebDriver.FilterElementsByText(visibleTextOfTheRelativeElement, _seleniumAdapter.WebDriver.GetAllElements(), false);

            var candidates = _query;

            foreach (var relative in relativeElements)
            {
                if (direction == SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo && 
                    _query.Any(q=>q.Equals(relative)))
                {
                    results.AddRange(_query.Where(q => q.Equals(relative)).Select(q=>new BufferedElement(q)));
                }

                var domNeighbours = _seleniumAdapter.WebDriver.FilterDomNeighbours(candidates, relative);
                //if (domNeighbours.Any())
                //{
                //    if (domNeighbours.Count() == 1)
                //    {
                //        results.Add(new BufferedElement(domNeighbours.First()));
                //    }
                //}

                IEnumerable<IBufferedElement> bufferedElements;
                if (orthogonalOnly)
                {
                    var orthogonalInputs = _seleniumAdapter.WebDriver.FilterOrthogonalElements(candidates, relative);
                    bufferedElements = _seleniumAdapter.WebDriver.SelectWithLocation(orthogonalInputs);
                }
                else
                {
                    bufferedElements = _seleniumAdapter.WebDriver.SelectWithLocation(candidates);
                }
                
                bufferedElements = SortByDistance(bufferedElements, relative.Location.X, relative.Location.Y);

                bufferedElements = PutNeighborsToFront(bufferedElements, domNeighbours);

                bufferedElements = FilterOtherDirections(direction, bufferedElements, relative);
                
                results.AddRange(bufferedElements);

                //Show.HighlightElements(bufferedElements.Select(x=>x.WebElement), Selenium);
            }

            return new LocationHeuristictSearchStrategy(_seleniumAdapter, new ReadOnlyCollection<IWebElement>(results.Select(x => x.WebElement).ToList()));
        }

        private static IEnumerable<IBufferedElement> FilterOtherDirections(SearchDirection direction, IEnumerable<IBufferedElement> bufferedElements,
            IWebElement relative)
        {
            switch (direction)
            {
                case SearchDirection.AboveAnotherElement:
                    bufferedElements = bufferedElements.Where(e => e.Rectangle.Top < relative.Location.Y);
                    break;
                case SearchDirection.BelowAnotherElement:
                    bufferedElements = bufferedElements.Where(e => e.Rectangle.Bottom > relative.Location.Y);
                    break;
                case SearchDirection.LeftFromAnotherElement:
                    bufferedElements = bufferedElements.Where(e => e.Rectangle.Right < relative.Location.X);
                    break;
                case SearchDirection.RightFromAnotherElement:
                    bufferedElements = bufferedElements.Where(e => e.Rectangle.Left > relative.Location.X);
                    break;
                case SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo:
                    var list  = bufferedElements.Where(e => e.Rectangle.Left > relative.Location.X).ToList();
                    list.AddRange(bufferedElements.Where(e => e.Rectangle.Left <= relative.Location.X));
                    bufferedElements = list;
                    break;
            }
            return bufferedElements;
        }

        public IElementSearchStrategy SortBy(SearchDirection specDirection)
        {
            var buffs = _query.Select(x => x.AsBufferedElement());
            IEnumerable<IBufferedElement> orderedEnumerable;
            switch (specDirection)
            {
                case SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo:
                    orderedEnumerable = buffs;
                    break;
                case SearchDirection.AboveAnotherElement:
                case SearchDirection.UpFromBottomEdge:
                    orderedEnumerable = buffs.OrderByDescending(e => e.Rectangle.Location.Y);
                    break;
                case SearchDirection.BelowAnotherElement:
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

            return new LocationHeuristictSearchStrategy(_seleniumAdapter, new ReadOnlyCollection<IWebElement>(orderedEnumerable.Select(x => x.WebElement).ToList()));
        }

        public IEnumerable<IBufferedElement> Results()
        {
            return _query.Select(x=>x.AsBufferedElement());
        }

        public bool Any()
        {
            return _query.Any();
        }

        public IBufferedElement FindElementNextToAnotherElement(string visibleTextOfTheRelativeElement, SearchDirection direction, bool orthogonalOnly)
        {
            var returnValue = default(IBufferedElement);

            var searchedTagNames = new[] {"input", "textArea"};

            var relativeElements = _seleniumAdapter.WebDriver.FilterElementsByText(visibleTextOfTheRelativeElement, _seleniumAdapter.WebDriver.GetAllElements(), false);
            
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

                IEnumerable<IBufferedElement> bufferedElements;

                if (orthogonalOnly)
                {
                    var orthogonalInputs = _seleniumAdapter.WebDriver.FilterOrthogonalElements(candidates, relative);
                    bufferedElements = _seleniumAdapter.WebDriver.SelectWithLocation(orthogonalInputs);
                }
                else
                {
                    bufferedElements = _seleniumAdapter.WebDriver.SelectWithLocation(candidates);
                }

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