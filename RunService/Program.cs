// See https://aka.ms/new-console-template for more information
using Origam.SigniService;
using Origam.Service.Core;
using System.Data;
using System.Xml;

SigniServiceAgent signiServiceAgent = new SigniServiceAgent();
signiServiceAgent.MethodName = "SendToSigni";
//api key pridat
signiServiceAgent.Parameters.Add("signing_order", "proposers_before_counterparties");
signiServiceAgent.Parameters.Add("autosign_proposers", "V Praze");

DataTable table = new DataTable();
//Creating columns
table.Columns.Add("Proposer", typeof(bool));
table.Columns.Add("Email", typeof(string));
table.Columns.Add("Contranct", typeof(string));
table.Columns.Add("PartyOrder", typeof(int));
table.Columns.Add("PersonType", typeof(string));
table.Columns.Add("FirstName", typeof(string));
table.Columns.Add("LastName", typeof(string));

//Adding data in a Datatable.
table.Rows.Add(true, "signi@bohemianestates.com", "sign");
table.Rows.Add(false, "jprajz@gmail.com", "sign",1,"nature","Jiří","Prajz");

IDataDocument dataDocument = new DataDocumentCore();
dataDocument.DataSet.Tables.Add(table);
signiServiceAgent.Parameters.Add("people", dataDocument);


DataTable templateTable = new DataTable();
//Creating columns
templateTable.Columns.Add("TemplateId", typeof(int));
templateTable.Columns.Add("Name", typeof(string));

templateTable.Rows.Add(821, "Testovací smlouva Jirka“");

DataTable parameteterTable = new DataTable();
//Creating columns
parameteterTable.Columns.Add("ParameterId", typeof(int));
parameteterTable.Columns.Add("ParameterValue", typeof(string));

parameteterTable.Rows.Add(112, "„Hnutí za digitální revoluci“");
parameteterTable.Rows.Add(122, "„Digitální revolucionář“");
parameteterTable.Rows.Add(131, "Chci spolupracovat");
parameteterTable.Rows.Add(411, "chci kocky");
parameteterTable.Rows.Add(421, "20.2.2022");
parameteterTable.Rows.Add(431, "Praha");

dataDocument = new DataDocumentCore();
dataDocument.DataSet.Tables.Add(templateTable);
dataDocument.DataSet.Tables.Add(parameteterTable);
signiServiceAgent.Parameters.Add("template", dataDocument);

signiServiceAgent.Run();
var result = signiServiceAgent.Result;
Console.WriteLine("Result " + ((XmlDocument)result).InnerXml);