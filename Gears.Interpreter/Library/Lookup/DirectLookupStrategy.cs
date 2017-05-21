using System;
using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Adapters;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library.Lookup
{
    public class DirectLookupStrategy : ILookupStrategy
    {
        private readonly ISeleniumAdapter _seleniumAdapter;
        private readonly List<ITagSelector> _searchedTagNames;
        private readonly string _subjectName;
        private readonly string _locale;
        private readonly SearchDirection _searchDirection;
        private readonly int _order;
        private readonly bool _orthogonalOnly;
        private readonly bool _exactMatchOnly;
        private int _staleElementCounter = 0;

        public DirectLookupStrategy(ISeleniumAdapter seleniumAdapter, 
            List<ITagSelector> searchedTagNames, 
            string subjectName, 
            string locale, 
            SearchDirection searchDirection, 
            int order, 
            bool orthogonalOnly, 
            bool exactMatchOnly = true)
        {
            _seleniumAdapter = seleniumAdapter;
            _searchedTagNames = searchedTagNames;
            _subjectName = subjectName;
            _locale = locale;
            _searchDirection = searchDirection;
            _order = order;
            _orthogonalOnly = orthogonalOnly;
            _exactMatchOnly = exactMatchOnly;
        }

        public ILookupResult LookUp()
        {
            try
            {
                var lookingForSpecificElements = _searchedTagNames.Any();

                var query = _seleniumAdapter.Query.Elements(_searchedTagNames).WithText(_subjectName, lookingForSpecificElements, _exactMatchOnly);

                var xTolerance = (_searchDirection == SearchDirection.AboveAnotherElement ||
                                  _searchDirection == SearchDirection.BelowAnotherElement)
                    ? 40
                    : 5;

                var yTolerance = (_searchDirection == SearchDirection.AboveAnotherElement ||
                                  _searchDirection == SearchDirection.BelowAnotherElement)
                    ? 5
                    : 40;

                if (!string.IsNullOrEmpty(_locale))
                {
                    query = query.RelativeTo(_locale, _searchDirection, _orthogonalOnly, xTolerance, yTolerance);
                }

                query = query.SortBy(_searchDirection);

                var validResults = query.RemoveDuplicatesAndDegenerates(query.Results());

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
                    LookUp();
                }

                throw new SeleniumException(e);
            }
        }
    }
}