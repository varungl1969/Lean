/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using QuantConnect.Data;
using QuantConnect.Securities;
using System;
using QuantConnect.Interfaces;
using System.Collections.Generic;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Regression algorithm to test we can specify a custom settlement model using Security.SetSettlementModel() method
    /// </summary>
    public class SetCustomSettlementModelRegressionAlgorithm: QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private Security _spy;
        public override void Initialize()
        {
            SetStartDate(2013, 10, 7);
            SetEndDate(2013, 10, 11);
            SetCash(10000);
            _spy = AddEquity("SPY", Resolution.Daily);
            _spy.SetSettlementModel(new CustomSettlementModel());
        }

        public override void OnData(Slice slice)
        {
            if (Portfolio.CashBook[Currencies.USD].Amount == 10000)
            {
                var parameters = new ApplyFundsSettlementModelParameters(Portfolio, _spy, Time, new CashAmount(101, Currencies.USD), null);
                _spy.SettlementModel.ApplyFunds(parameters);
            }
        }

        public override void OnEndOfAlgorithm()
        {
            if (Portfolio.CashBook[Currencies.USD].Amount != 10101)
            {
                throw new Exception($"It was expected to have 10101 USD in Portfolio, but was {Portfolio.CashBook[Currencies.USD].Amount}");
            }

            var parameters = new ScanSettlementModelParameters(Portfolio, _spy, new DateTime(2013, 10, 6));
            _spy.SettlementModel.Scan(parameters);

            if (Portfolio.CashBook[Currencies.USD].Amount != 10000)
            {
                throw new Exception($"It was expected to have 10000 USD in Portfolio, but was {Portfolio.CashBook[Currencies.USD].Amount}");
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public long DataPoints => 48;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 0;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "0"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "108.257%"},
            {"Drawdown", "0%"},
            {"Expectancy", "0"},
            {"Net Profit", "1.010%"},
            {"Sharpe Ratio", "10.983"},
            {"Sortino Ratio", "0"},
            {"Probabilistic Sharpe Ratio", "95.977%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "1.42"},
            {"Beta", "-0.273"},
            {"Annual Standard Deviation", "0.08"},
            {"Annual Variance", "0.006"},
            {"Information Ratio", "-3.801"},
            {"Tracking Error", "0.288"},
            {"Treynor Ratio", "-3.226"},
            {"Total Fees", "$0.00"},
            {"Estimated Strategy Capacity", "$0"},
            {"Lowest Capacity Asset", ""},
            {"Portfolio Turnover", "0%"},
            {"OrderListHash", "d41d8cd98f00b204e9800998ecf8427e"}
        };
    }

    public class CustomSettlementModel : ISettlementModel
    {
        private string _currency;
        private decimal _amount;
        public void ApplyFunds(ApplyFundsSettlementModelParameters applyFundsParameters)
        {
            _currency = applyFundsParameters.CashAmount.Currency;
            _amount = applyFundsParameters.CashAmount.Amount;
            applyFundsParameters.Portfolio.CashBook[_currency].AddAmount(_amount);
        }

        public void Scan(ScanSettlementModelParameters settlementParameters)
        {
            if (settlementParameters.UtcTime == new DateTime(2013, 10, 6))
            {
                settlementParameters.Portfolio.CashBook[_currency].AddAmount(-_amount);
            }
        }

        /// <summary>
        /// Gets the unsettled cash amount for the security
        /// </summary>
        public CashAmount GetUnsettledCash()
        {
            return default;
        }
    }
}
