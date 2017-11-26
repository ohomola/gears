﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Gears.Interpreter.Core.Adapters.UI.JavaScripts;
using OpenQA.Selenium;

namespace Gears.Interpreter.Core.Adapters.UI.Lookup
{
    public interface IFluentElementQuery
    {
        IFluentElementQuery Elements(IEnumerable<string> searchedTagNames);
        IFluentElementQuery WithText(string text, bool matchWhenTextIsInChild, bool exactMatchOnly);
        IFluentElementQuery RelativeTo(string text, SearchDirection specDirection, bool orthogonalOnly, int xTolerance = 5, int yTolerance = 5);
        IFluentElementQuery RelativeTo(IWebElement relative, SearchDirection specDirection, bool orthogonalOnly, int xTolerance = 5, int yTolerance = 5);
        IFluentElementQuery SortBy(SearchDirection specDirection);
        IEnumerable<IBufferedElement> Results();
        bool Any();
        IFluentElementQuery Elements();
        IEnumerable<IBufferedElement> RemoveDuplicatesAndDegenerates(IEnumerable<IBufferedElement> validResults);
        IFluentElementQuery Elements(IEnumerable<ITagSelector> searchedTagNames);
        IEnumerable<IBufferedElement> OnScreen(IEnumerable<IBufferedElement> bufferedElements);
    }

    public class FluentElementQuery : IFluentElementQuery
    {
        private readonly ISeleniumAdapter _seleniumAdapter;

        private readonly ReadOnlyCollection<IWebElement> _query;

        private static int _staleElementCounter = 0;

        public FluentElementQuery(ISeleniumAdapter seleniumAdapter)
        {
            _seleniumAdapter = seleniumAdapter;
        }

        private FluentElementQuery(ISeleniumAdapter seleniumAdapter, ReadOnlyCollection<IWebElement> query)
        {
            _seleniumAdapter = seleniumAdapter;
            _query = new ReadOnlyCollection<IWebElement>(new List<IWebElement>(query));
        }

        public IEnumerable<IBufferedElement> RemoveDuplicatesAndDegenerates(IEnumerable<IBufferedElement> validResults)
        {
            return new HashSet<IBufferedElement>(validResults.Where(x => x.Rectangle.Height > 0 && x.Rectangle.Width > 0));
        }

        public IFluentElementQuery Elements()
        {
            return Elements(new List<string>());
        }

        public IFluentElementQuery Elements(IEnumerable<string> searchedTagNames)
        {
            return Elements(searchedTagNames.Select(x => new TagNameSelector(x)));
        }

        public IFluentElementQuery Elements(IEnumerable<ITagSelector> searchedTagNames)
        {
            searchedTagNames = searchedTagNames.ToList();

            if (!searchedTagNames.Any())
            {
                return new FluentElementQuery(_seleniumAdapter, _seleniumAdapter.WebDriver.GetAllElements());
            }

            var newQuery = new List<IWebElement>();

            var tagNameSelectors = searchedTagNames.OfType<TagNameSelector>();
            if (tagNameSelectors.Any())
            {
                newQuery = _seleniumAdapter.WebDriver.GetElementsByTagNames(tagNameSelectors.Select(x => x.TagName)).ToList();
            }

            var attributeSelectors = searchedTagNames.OfType<AttributeSelector>();
            if (attributeSelectors.Any())
            {
                newQuery.AddRange(_seleniumAdapter.WebDriver.GetElementsByAttributeValues(attributeSelectors.Select(x => x.Name).ToList(), attributeSelectors.Select(x => x.Value).ToList()));
            }

            return new FluentElementQuery(_seleniumAdapter, new ReadOnlyCollection<IWebElement>(newQuery));
        }

        public IFluentElementQuery WithText(string text, bool matchWhenTextIsInChild, bool exactMatchOnly = true)
        {
            var newQuery = _query;

            if (!string.IsNullOrEmpty(text))
            {
                if (exactMatchOnly)
                {
                    newQuery = _seleniumAdapter.WebDriver.FilterElementsByText(text, _query, matchWhenTextIsInChild);
                }
                else
                {
                    newQuery = _seleniumAdapter.WebDriver.FilterElementsByPartialText(text, _query, matchWhenTextIsInChild);
                }

            }

            return new FluentElementQuery(_seleniumAdapter, newQuery);
        }

        public IFluentElementQuery RelativeTo(IWebElement relative, SearchDirection direction, bool orthogonalOnly, int xTolerance = 5, int yTolerance = 5)
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

            return new FluentElementQuery(_seleniumAdapter, new ReadOnlyCollection<IWebElement>(results.Select(x => x.WebElement).ToList()));
        }

        public IFluentElementQuery RelativeTo(string visibleTextOfTheRelativeElement, SearchDirection direction, bool orthogonalOnly, int xTolerance = 5, int yTolerance = 5)
        {
            if (string.IsNullOrEmpty(visibleTextOfTheRelativeElement))
            {
                return new FluentElementQuery(_seleniumAdapter, _query);
            }

            var results = new List<IBufferedElement>();
            var relativeElements = _seleniumAdapter.WebDriver.FilterElementsByText(visibleTextOfTheRelativeElement, _seleniumAdapter.WebDriver.GetAllElements(), false);

            var relativeBufferedElements = OnScreen(_seleniumAdapter.WebDriver.SelectWithLocation(relativeElements));

            var candidates = _query;

            foreach (var relative in relativeBufferedElements)
            {
                if (direction == SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo &&
                    _query.Any(q => q.Equals(relative)))
                {
                    results.AddRange(_query.Where(q => q.Equals(relative)).Select(q => new BufferedElement(q)));
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

            return new FluentElementQuery(_seleniumAdapter, new ReadOnlyCollection<IWebElement>(results.Select(x => x.WebElement).ToList()));
        }

        public IEnumerable<IBufferedElement> OnScreen(IEnumerable<IBufferedElement> bufferedElements)
        {
            //var chromeHandle = _seleniumAdapter.BrowserHandle();
            //var browserBox = new UserBindings.RECT();
            //UserBindings.GetWindowRect(chromeHandle, ref browserBox);
            //foreach (var e in bufferedElements)
            //{
            //    _seleniumAdapter.PutElementOnScreen(e.WebElement);

            //    var refreshedPosition = e.WebElement.AsBufferedElement().Rectangle;

            //    var centerX = refreshedPosition.X + refreshedPosition.Width / 2;
            //    var centerY = refreshedPosition.Y + refreshedPosition.Height / 2;

            //    var p = new Point(centerX, centerY);
            //    _seleniumAdapter.ConvertFromPageToWindow(ref p);

            //    if (p.X >= 0 && p.X <= browserBox.Right && p.Y >= 0 && p.Y <= browserBox.Bottom)
            //    {
            //        yield return e;
            //    }
            //}

            return bufferedElements;
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
                    var list = bufferedElements.Where(e => e.Rectangle.Right > relative.Location.X).ToList();
                    list.AddRange(bufferedElements.Where(e => e.Rectangle.Right <= relative.Location.X));
                    bufferedElements = list;
                    break;
            }
            return bufferedElements;
        }

        public IFluentElementQuery SortBy(SearchDirection specDirection)
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

            return new FluentElementQuery(_seleniumAdapter, new ReadOnlyCollection<IWebElement>(orderedEnumerable.Select(x => x.WebElement).ToList()));
        }

        public IEnumerable<IBufferedElement> Results()
        {
            return _query.Select(x => x.AsBufferedElement());
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
}