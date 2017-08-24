using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace Gears.Interpreter.Core.Adapters.UI.Lookup
{
    public class DirectLookupStrategyWithNeighbours : ILookupStrategy
    {
        private readonly ISeleniumAdapter _seleniumAdapter;
        private int _staleElementCounter = 0;
        private SearchDirection _searchDirection;
        private string _labelText;
        private bool _exactMatchOnly;
        private int _order;
        private List<ITagSelector> _searchedTagNames;

        public DirectLookupStrategyWithNeighbours(ISeleniumAdapter seleniumAdapter,
            string labelText, SearchDirection searchDirection, int order, List<ITagSelector> searchedTagNames, bool exactMatchOnly = true)
        {
            _seleniumAdapter = seleniumAdapter;
            _labelText = labelText;
            _searchDirection = searchDirection;
            _order = order;
            _exactMatchOnly = exactMatchOnly;
            _searchedTagNames = searchedTagNames;
        }

        public ILookupResult LookUp()
        {
            try
            {
                IEnumerable<IBufferedElement> validResults;
                var query = _seleniumAdapter.Query.Elements(_searchedTagNames);

                var allInputsWithText = query.WithText(_labelText, matchWhenTextIsInChild: false, exactMatchOnly: _exactMatchOnly);

                if (allInputsWithText.Any())
                {
                    validResults = allInputsWithText
                        .SortBy(_searchDirection)
                        .Results();
                }
                else
                {
                    var xTolerance = (_searchDirection == SearchDirection.AboveAnotherElement ||
                                  _searchDirection == SearchDirection.BelowAnotherElement)
                    ? 40
                    : 5;

                    var yTolerance = (_searchDirection == SearchDirection.AboveAnotherElement ||
                                      _searchDirection == SearchDirection.BelowAnotherElement)
                        ? 5
                        : 40;

                    validResults = query
                        .RelativeTo(_labelText, _searchDirection, true, xTolerance, yTolerance)
                        .SortBy(_searchDirection)
                        .Results();
                }

                validResults = query.RemoveDuplicatesAndDegenerates(validResults);

                validResults = query.OnScreen(validResults);

                try
                {
                    return new LookupResult(validResults, validResults.Skip(_order).First(), success: true);
                }
                catch (Exception)
                {
                    return new LookupResult(validResults, mainResult: null, success: false);
                }
            }
            catch (StaleElementReferenceException e)
            {
                _staleElementCounter++;
                if (_staleElementCounter < 10)
                {
                    return LookUp();
                }
                throw new SeleniumException(e);
            }
        }
    }
}