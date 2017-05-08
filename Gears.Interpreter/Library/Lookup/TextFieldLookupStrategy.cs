using System;
using System.Collections.Generic;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Core;

namespace Gears.Interpreter.Library.Lookup
{
    public class TextFieldLookupStrategy : ILookupStrategy
    {
        private readonly ISeleniumAdapter _seleniumAdapter;
        private List<ITagSelector> _searchedTagNames;

        public bool ExactMatchOnly { get; set; }

        public int Order { get; set; }

        public SearchDirection Direction { get; set; }

        public string LabelText { get; set; }

        public string Locale { get; set; }

        public TextFieldLookupStrategy(ISeleniumAdapter seleniumAdapter, bool exactMatchOnly, int order, SearchDirection direction, string labelText, List<ITagSelector> searchedTagNames)
        {
            _seleniumAdapter = seleniumAdapter;
            ExactMatchOnly = exactMatchOnly;
            Order = order;
            Direction = direction;
            LabelText = labelText;
            _searchedTagNames = searchedTagNames;
        }

        public ILookupResult LookUp()
        {
            // Try Direct match first (placeholder text, attribute or anything right inside the input)
            var searchDirectly = new DirectLookupStrategy(
                _seleniumAdapter,
                _searchedTagNames,
                LabelText,
                Locale,
                Direction,
                Order,
                false,
                true).LookUp();

            if (searchDirectly.Success)
            {
                return searchDirectly;
            }

            // Try Exact match with neighbour labels
            var nextToLabel = new DirectLookupStrategyWithNeighbours(
                _seleniumAdapter, 
                LabelText, 
                Direction,
                Order, _searchedTagNames).LookUp();

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
                    Order, _searchedTagNames, 
                    false).LookUp();
            }

            if (nextToLabel.Success)
            {
                return nextToLabel;
            }

            throw new LookupFailureException(nextToLabel, "Input not found");
        }

        
    }
}