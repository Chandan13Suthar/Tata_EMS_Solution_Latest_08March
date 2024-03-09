#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.DataLogger;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Store;
using FTOptix.ODBCStore;
using FTOptix.Report;
using FTOptix.RAEtherNetIP;
using FTOptix.Retentivity;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using Store = FTOptix.Store;
using System.Text.RegularExpressions;
using FTOptix.SQLiteStore;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection.Emit;
using FTOptix.MicroController;
using System.Collections.Generic;
using System.Collections;
#endregion
public class RuntimeNetLogic5 : BaseNetLogic
{


    public override void Start()
    {
        var owner = (HourlyDataAgg)LogicObject.Owner;


        dateVariable = owner.DateVariable;
        buttonVariable = owner.ButtonVariable;
        consumptionVariable = owner.ConsumptionVariable;
        jaceVariable = owner.JaceVariable;
        meterVariable = owner.MeterVariable;

        periodicTask = new PeriodicTask(IncrementDecrementTask, 2000, LogicObject);
        periodicTask.Start();
    }

    public override void Stop()
    {

        periodicTask.Dispose();
        periodicTask = null;
    }

    public void IncrementDecrementTask()
    {
        string meter = meterVariable.Value;

        bool button = buttonVariable.Value;
        var project = FTOptix.HMIProject.Project.Current;
        var myStore1 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");
        var myStore2 = project.GetObject("DataStores").Get<Store.Store>("ODBCDatabase");

        object[,] resultSet1;
        string[] header1;

        object[,] resultSet2;
        string[] header2;


        if (button == true)
        {
            string currentHour = DateTime.Now.ToString("HH");
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            // string currentTime = DateTime.Now.AddSeconds(-1).ToString();
            string query1 = $"DELETE FROM DailyConsumption WHERE LocalTimestamp BETWEEN '" + currentDate + " " + currentHour + ":00:00' AND '" + currentDate + " " + currentHour + ":59:59' AND Meter = 'INCOMER2'";
            string query2 = $" SELECT Meter FROM HomePage WHERE LocalTimestamp BETWEEN '" + currentDate + " " + currentHour + ":00:00' AND '" + currentDate + " " + currentHour + ":59:59' AND Jace = '33KV' AND Meter = 'INCOMER2'";
            // string query1 = $"SELECT Meter FROM HomePage WHERE Jace = '33KV' AND Meter = 'INCOMER2'";
            // string query1 = $" SELECT Meter FROM HomePage WHERE Jace = '33KV' AND Meter = 'INCOMER2' AND DATEPART(HOUR, LocalTimestamp) = @currentHour";

            myStore1.Query(query1, out header1, out resultSet1);
            myStore2.Query(query2, out header2, out resultSet2);

            /*  DELETE FROM DailyConsumption WHERE Jace = 'UTILITY'      AND Meter = 'MCB_MVS_01_IN1'
      AND Date = '{currentHour}';
          */
            //  throw new Exception(query1);

            var rowCount1 = resultSet1 != null ? resultSet1.GetLength(0) : 0;
            var columnCount1 = header1 != null ? header1.Length : 0;
            if (rowCount1 > 0 && columnCount1 > 0)
            {
                var column1 = Convert.ToString(resultSet1[0, 0]);
                meter = column1;

            }


            var rowCount2 = resultSet2 != null ? resultSet2.GetLength(0) : 0;
            var columnCount2 = header2 != null ? header2.Length : 0;
            if (rowCount2 > 0 && columnCount2 > 0)
            {
                var column1 = Convert.ToString(resultSet2[0, 0]);
                meter = column1;

            }

            meterVariable.Value = meter;
        }
        //  throw new Exception(meter);
    }



    private IUAVariable dateVariable;
    private IUAVariable buttonVariable;
    private IUAVariable consumptionVariable;
    private IUAVariable jaceVariable;
    private IUAVariable meterVariable;
    private PeriodicTask periodicTask;


}