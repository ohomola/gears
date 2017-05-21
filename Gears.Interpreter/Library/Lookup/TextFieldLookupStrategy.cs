using System;
using System.Collections.Generic;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Core;

namespace Gears.Interpreter.Library.Lookup
{
    public class TextFieldLookupStrategy : ILookupStrategy
    {
        private readonly ISeleniumAdapter _seleniumAdapter;

        public bool ExactMatchOnly { get; set; }

        public int Order { get; set; }

        public SearchDirection Direction { get; set; }

        public string LabelText { get; set; }

        public string Locale { get; set; }

        public TextFieldLookupStrategy(ISeleniumAdapter seleniumAdapter, bool exactMatchOnly, int order, SearchDirection direction, string labelText)
        {
            _seleniumAdapter = seleniumAdapter;
            ExactMatchOnly = exactMatchOnly;
            Order = order;
            Direction = direction;
            LabelText = labelText;
        }

        public ILookupResult LookUp()
        {
            // Try Direct match first (placeholder text, attribute or anything right inside the input)
            var searchDirectly = new DirectLookupStrategy(
                _seleniumAdapter,
                new List<ITagSelector>
                {
                    new TagNameSelector("input"),
                    new TagNameSelector("textArea"),
                    new TagNameSelector("label")
                },
                LabelText,
                Locale,
                Direction,
                Order,
                false,
                true).LookUp();

            // Only match it if it's cursor is input (it reacts focus on user click)
            var cursorType = GetCursorType(searchDirectly);
            if (searchDirectly.Success && cursorType.ToLower() == "input" || cursorType.ToLower() == "pointer")
            {
                return searchDirectly;
            }

            // Try Exact match with neighbour labels
            var nextToLabel = new DirectLookupStrategyWithNeighbours(
                _seleniumAdapter, 
                LabelText, 
                Direction,
                Order, 
                new List<ITagSelector>
                {
                    new TagNameSelector("input"),
                    new TagNameSelector("textArea")
                }).LookUp();

            if (nextToLabel.Success)
            {
                return nextToLabel;
            }

            // Try Nonexact match with neighbour labels
            if (ExactMatchOnly == false)
            {
                nextToLabel = new DirectLookupStrategyWithNeighbours(
                    _seleniumAdapter, 
                    LabelText, 
                    Direction, 
                    Order, new List<ITagSelector>
                    {
                        new TagNameSelector("input"),
                        new TagNameSelector("textArea")
                    }, 
                    false).LookUp();
            }

            if (nextToLabel.Success)
            {
                return nextToLabel;
            }

            throw new LookupFailureException(nextToLabel, "Input not found");
        }

        private static string GetCursorType(ILookupResult searchDirectly)
        {
            if (searchDirectly.Success == false)
            {
                return string.Empty;
            }

            if (searchDirectly.MainResult == null)
            {
                return string.Empty;
            }

            if (searchDirectly.MainResult.WebElement == null)
            {
                return string.Empty;
            }

            return searchDirectly.MainResult.WebElement.GetCssValue("cursor");
        }
    }
}