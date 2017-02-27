using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications.Debugging;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public interface IElementSearchStrategy
    {
        LookupResult DirectLookup(List<ITagSelector> searchedTagNames, string subjectName, string locale, SearchDirection searchDirection, int Order, bool orthogonalOnly);

        IElementSearchStrategy Elements(IEnumerable<string> searchedTagNames);
        IElementSearchStrategy WithText(string text, bool matchWhenTextIsInChild);
        IElementSearchStrategy RelativeTo(string text, SearchDirection specDirection, bool orthogonalOnly, int xTolerance = 5, int yTolerance = 5);
        IElementSearchStrategy RelativeTo(IWebElement relative, SearchDirection specDirection, bool orthogonalOnly, int xTolerance = 5, int yTolerance = 5);
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

        public LookupResult(IEnumerable<IBufferedElement> otherValidResults, IBufferedElement result, bool success)
        {
            OtherValidResults = otherValidResults;
            Result = result;
            Success = success;
        }

        public IEnumerable<IBufferedElement> OtherValidResults { get; set; }
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

        private static int _staleElementCounter = 0;
        public LookupResult DirectLookup(List<ITagSelector> searchedTagNames, string subjectName, string locale, SearchDirection searchDirection, int order, bool orthogonalOnly)
        {
            try
            {
                var lookingForSpecificElements = searchedTagNames.Any();

                var query = Elements(searchedTagNames).WithText(subjectName, lookingForSpecificElements);

                var xTolerance = (searchDirection == SearchDirection.AboveAnotherElement ||
                                  searchDirection == SearchDirection.BelowAnotherElement)
                    ? 40
                    : 5;

                var yTolerance = (searchDirection == SearchDirection.AboveAnotherElement ||
                                  searchDirection == SearchDirection.BelowAnotherElement)
                    ? 5
                    : 40;

                if (!string.IsNullOrEmpty(locale))
                {
                    query = query.RelativeTo(locale, searchDirection, orthogonalOnly, xTolerance, yTolerance);
                }

                query = query.SortBy(searchDirection);

                var validResults = RemoveDuplicatesAndDegenerates(query.Results());

                validResults = OnScreen(validResults);

                try
                {
                    return new LookupResult(validResults, validResults.Skip(order).First(), success: true);
                }
                catch (Exception)
                {
                    return new LookupResult(validResults, result: null, success: false);
                }
            }
            catch (StaleElementReferenceException)
            {
                _staleElementCounter++;
                if (_staleElementCounter < 10)
                {
                    return DirectLookup(searchedTagNames, subjectName, locale, searchDirection, order, orthogonalOnly);
                }
                throw;
            }
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
            try
            {
                IEnumerable<IBufferedElement> validResults;
                var allInputs = Elements(new[] { "input", "textArea" });

                var allInputsWithText = allInputs.WithText(labelText, matchWhenTextIsInChild: false);

                if (allInputsWithText.Any())
                {
                    validResults = allInputsWithText
                        .SortBy(searchDirection)
                        .Results();
                }
                else
                {
                    var xTolerance = (searchDirection == SearchDirection.AboveAnotherElement ||
                                  searchDirection == SearchDirection.BelowAnotherElement)
                    ? 40
                    : 5;

                    var yTolerance = (searchDirection == SearchDirection.AboveAnotherElement ||
                                      searchDirection == SearchDirection.BelowAnotherElement)
                        ? 5
                        : 40;

                    validResults = allInputs
                        .RelativeTo(labelText, searchDirection, true, xTolerance, yTolerance)
                        .SortBy(searchDirection)
                        .Results();
                }

                validResults = RemoveDuplicatesAndDegenerates(validResults);

                validResults = OnScreen(validResults);

                try
                {
                    return new LookupResult(validResults, validResults.Skip(order).First(), success: true);
                }
                catch (Exception)
                {
                    return new LookupResult(validResults, result: null, success: false);
                }
            }
            catch (StaleElementReferenceException)
            {
                _staleElementCounter++;
                if (_staleElementCounter < 10)
                {
                    return DirectLookupWithNeighbours(labelText, searchDirection, order);
                }
                throw;
            }
        }

        private IEnumerable<IBufferedElement> RemoveDuplicatesAndDegenerates(IEnumerable<IBufferedElement> validResults)
        {
            return new HashSet<IBufferedElement>(validResults.Where(x=>x.Rectangle.Height >0 && x.Rectangle.Width > 0));
        }

        public IElementSearchStrategy Elements()
        {
            return Elements(new List<string>());
        }

        public IElementSearchStrategy Elements(IEnumerable<string> searchedTagNames)
        {
            return Elements(searchedTagNames.Select(x=>new TagNameSelector(x)));
        }

        public IElementSearchStrategy Elements(IEnumerable<ITagSelector> searchedTagNames)
        {
            searchedTagNames = searchedTagNames.ToList();

            if (!searchedTagNames.Any())
            {
                return new LocationHeuristictSearchStrategy(_seleniumAdapter, _seleniumAdapter.WebDriver.GetAllElements());
            }

            var newQuery = new List<IWebElement>();

            var tagNameSelectors = searchedTagNames.OfType<TagNameSelector>();
            if (tagNameSelectors.Any())
            {
                newQuery = _seleniumAdapter.WebDriver.GetElementsByTagNames(tagNameSelectors.Select(x=>x.TagName)).ToList();
            }

            var attributeSelectors = searchedTagNames.OfType<AttributeSelector>();
            if (attributeSelectors.Any())
            {
                newQuery.AddRange(_seleniumAdapter.WebDriver.GetElementsByAttributeValues(attributeSelectors.Select(x=>x.Name).ToList(), attributeSelectors.Select(x => x.Value).ToList()));
            }

            return new LocationHeuristictSearchStrategy(_seleniumAdapter, new ReadOnlyCollection<IWebElement>(newQuery));
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

        public IElementSearchStrategy RelativeTo(IWebElement relative, SearchDirection direction, bool orthogonalOnly, int xTolerance = 5, int yTolerance = 5)
        {
            //var candidates = new ReadOnlyCollection<IWebElement>(new List<IWebElement>() {relative});

            var candidates = _query;

            var results = new List<IBufferedElement>();
            {
                if (direction == SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo &&
                    _query.Any(q => q.Equals(relative)))
                {
                    results.AddRange(_query.Where(q => q.Equals(relative)).Select(q => new BufferedElement(q)));
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
                    var orthogonalInputs = _seleniumAdapter.WebDriver.FilterOrthogonalElements(candidates, relative, xTolerance, yTolerance);
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

                //Highlighter.HighlightElements(bufferedElements.Select(x=>x.WebElement), Selenium);
            }

            return new LocationHeuristictSearchStrategy(_seleniumAdapter, new ReadOnlyCollection<IWebElement>(results.Select(x => x.WebElement).ToList()));
        }

        public IElementSearchStrategy RelativeTo(string visibleTextOfTheRelativeElement, SearchDirection direction, bool orthogonalOnly, int xTolerance =5, int yTolerance = 5)
        {
            if (string.IsNullOrEmpty(visibleTextOfTheRelativeElement))
            {
                return new LocationHeuristictSearchStrategy(_seleniumAdapter, _query);
            }

            var results = new List<IBufferedElement>();
            var relativeElements = _seleniumAdapter.WebDriver.FilterElementsByText(visibleTextOfTheRelativeElement, _seleniumAdapter.WebDriver.GetAllElements(), false);

            var relativeBufferedElements = OnScreen(_seleniumAdapter.WebDriver.SelectWithLocation(relativeElements));

            var candidates = _query;

            foreach (var relative in relativeBufferedElements)
            {
                if (direction == SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo && 
                    _query.Any(q=>q.Equals(relative)))
                {
                    results.AddRange(_query.Where(q => q.Equals(relative)).Select(q=>new BufferedElement(q)));
                }

                var domNeighbours = _seleniumAdapter.WebDriver.FilterDomNeighbours(candidates, relative.WebElement);
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
                    var orthogonalInputs = _seleniumAdapter.WebDriver.FilterOrthogonalElements(candidates, relative.WebElement, xTolerance, yTolerance);
                    bufferedElements = _seleniumAdapter.WebDriver.SelectWithLocation(orthogonalInputs);
                }
                else
                {
                    bufferedElements = _seleniumAdapter.WebDriver.SelectWithLocation(candidates);
                }
                
                bufferedElements = SortByDistance(bufferedElements, relative.Rectangle.X, relative.Rectangle.Y);

                bufferedElements = PutNeighborsToFront(bufferedElements, domNeighbours);

                bufferedElements = FilterOtherDirections(direction, bufferedElements, relative.WebElement);
                
                results.AddRange(bufferedElements);

                //Highlighter.HighlightElements(bufferedElements.Select(x=>x.WebElement), Selenium);
            }

            return new LocationHeuristictSearchStrategy(_seleniumAdapter, new ReadOnlyCollection<IWebElement>(results.Select(x => x.WebElement).ToList()));
        }

        private IEnumerable<IBufferedElement> OnScreen(IEnumerable<IBufferedElement> bufferedElements)
        {
            var chromeHandle = _seleniumAdapter.GetChromeHandle();
            var browserBox = new UserBindings.RECT();
            UserBindings.GetWindowRect(chromeHandle, ref browserBox);
            foreach (var e in bufferedElements)
            {
                _seleniumAdapter.PutElementOnScreen(e.WebElement);

                var refreshedPosition = e.WebElement.AsBufferedElement().Rectangle;

                var centerX = refreshedPosition.X + refreshedPosition.Width / 2;
                var centerY = refreshedPosition.Y + refreshedPosition.Height / 2;

                var p = new Point(centerX, centerY);
                _seleniumAdapter.ConvertFromPageToWindow(ref p);

                if (p.X >= 0 && p.X <= browserBox.Right && p.Y >= 0 && p.Y <= browserBox.Bottom)
                {
                    yield return e;
                }
            }
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
                    var list  = bufferedElements.Where(e => e.Rectangle.Right > relative.Location.X).ToList();
                    list.AddRange(bufferedElements.Where(e => e.Rectangle.Right <= relative.Location.X));
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

    public interface ITagSelector
    {
    }

    class TagNameSelector : ITagSelector
    {
        public TagNameSelector(string tagName)
        {
            TagName = tagName;
        }

        public string TagName { get; set; }

        public override string ToString()
        {
            return $"Tag = {TagName}";
        }
    }

    class AttributeSelector : ITagSelector
    {
        public AttributeSelector(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Name} = {Value}";
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}