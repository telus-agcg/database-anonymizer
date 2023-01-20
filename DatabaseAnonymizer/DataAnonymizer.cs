using Dapper;
using DataMasker;
using DataMasker.Interfaces;
using DataMasker.Models;
using DataMasker.Utils;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DatabaseAnonymizer
{
    public class DataAnonymizer
    {
        private readonly Config _config;
        private readonly string[] _executeBeforeMasking;

        public DataAnonymizer(Config config, string[] executeBeforeMasking)
        {
            _config = config;
            _executeBeforeMasking = executeBeforeMasking;
        }
        public async Task Start()
        {
            var swTotal = Stopwatch.StartNew();
            var sqlConnection = new SqlConnection(_config.DataSource.Config.connectionString.ToString());

            var dataProviders = new List<IDataProvider>
            {
                new BogusDataProvider(_config.DataGeneration),
                new SqlDataProvider(sqlConnection)
            };

            IDataMasker dataMasker = new DataMasker.DataMasker(dataProviders);

            IDataSource dataSource = DataSourceProvider.Provide(_config.DataSource.Type, _config.DataSource);

            var swScripts = Stopwatch.StartNew();

            if (_executeBeforeMasking != null)
            {
                Log.Information($"Found {_executeBeforeMasking.Length} custom scripts. Executing...");

                for (int i = 0; i < _executeBeforeMasking.Length; i++)
                {
                    await sqlConnection.ExecuteAsync(_executeBeforeMasking[i], commandType: CommandType.Text, commandTimeout: 600);
                    Log.Information($"{i + 1,2} of {_executeBeforeMasking.Length} Done");
                }
                swScripts.Stop();
                Log.Information($"Custom scripts executed in: {swScripts.Elapsed}");
            }

            var swTable = Stopwatch.StartNew();

            foreach (TableConfig tableConfig in _config.Tables)
            {
                var count = await dataSource.GetCount(tableConfig);
                Log.Information($"{tableConfig.Name}: {count} rows.{(string.IsNullOrEmpty(tableConfig.Criteria) ? string.Empty : $" Criteria: '{tableConfig.Criteria}'")}");

                IEnumerable<IDictionary<string, object>> rowsFromDb = await dataSource.GetData(tableConfig);
                List<IDictionary<string, object>> rows = rowsFromDb.ToList();
                count = rows.Count;
                if (count > 0)
                {
                    foreach (IDictionary<string, object> row in rows)
                    {
                        try
                        {
                            dataMasker.Mask(row, tableConfig);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, $"{tableConfig.Name} faulted on masking.");
                        }
                    }
                    try
                    {
                        const int interval = 10;
                        int notifyAt = interval;

                        await dataSource.UpdateRows(rows, 100, tableConfig, (updatedCount) =>
                        {
                            int progress = updatedCount * 100 / count;
                            if (progress >= notifyAt)
                            {
                                notifyAt += interval;
                                Log.Information($"{tableConfig.Name}: {progress}%");
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"{tableConfig.Name} faulted on updating.");
                    }

                }
                swTable.Stop();
                Log.Information($"{tableConfig.Name} anonymized in: {swTable.Elapsed}");
                swTable.Restart();
            }
            Log.Information($"Total time spent on processing all tables: {swTotal.Elapsed}");
        }
    }
}
