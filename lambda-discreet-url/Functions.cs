using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lambda.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Lambda
{
  public class Functions
  {
    // The name of the environment variable to set the DynamoDB table
    const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "UrlTable";

    IDynamoDBContext DDBContext { get; set; }

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Functions()
    {
      // Check if table name exists in the environment variables and if so add the table mapping.
      var name = System.Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
      if (!string.IsNullOrEmpty(name))
      {
        AWSConfigsDynamoDB.Context.TypeMappings[typeof(Record)] = new Amazon.Util.TypeMapping(typeof(Record), name);
      }

      var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
      this.DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
    }

    /// <summary>
    /// Constructor used for testing passing in a preconfigured DynamoDB client.
    /// </summary>
    /// <param name="ddbClient"></param>
    /// <param name="tableName"></param>
    public Functions(IAmazonDynamoDB ddbClient, string tableName)
    {
      if (!string.IsNullOrEmpty(tableName))
      {
        AWSConfigsDynamoDB.Context.TypeMappings[typeof(Record)] = new Amazon.Util.TypeMapping(typeof(Record), tableName);
      }

      var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
      this.DDBContext = new DynamoDBContext(ddbClient, config);
    }

    /// <summary>
    /// A Lambda function that returns the url identified by the guid
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> GetUrlAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
      string id = (new Uri(request.Path).Segments.Last()).ToString();

      try
      {
        Guid.Parse(id);
      }
      catch (ArgumentNullException e)
      {
        return new APIGatewayProxyResponse
        {
          StatusCode = (int)HttpStatusCode.BadRequest,
          Body = $"Url is missing identifier segment. {e.ToString()}"
        };
      }
      catch (FormatException e)
      {
        return new APIGatewayProxyResponse
        {
          StatusCode = (int)HttpStatusCode.BadRequest,
          Body = $"Url is missing identifier segment. {e.ToString()}"
        };
      }

      context.Logger.LogLine($"Fetching url {id}");
      var record = await DDBContext.LoadAsync<Record>(id);
      context.Logger.LogLine($"Found url: {record != null}");

      if (record == null)
      {
        return new APIGatewayProxyResponse
        {
          StatusCode = (int)HttpStatusCode.NotFound
        };
      }

      return new APIGatewayProxyResponse
      {
        StatusCode = (int)HttpStatusCode.Redirect,
        Body = null,
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" }, { "Location", record.Url } }
      };
    }

    /// <summary>
    /// A Lambda function that adds a record.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> AddUrlAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
      var model = JsonConvert.DeserializeObject<APIModel>(request?.Body);

      var record = new Record
      {
        Id = Guid.NewGuid().ToString(),
        Url = model.Url
      };

      context.Logger.LogLine($"Saving record with id {record.Id}");
      await DDBContext.SaveAsync<Record>(record);

      model.Url = $"https://www.hide.com/{record.Id}";

      return new APIGatewayProxyResponse
      {
        StatusCode = (int)HttpStatusCode.OK,
        Body = JsonConvert.SerializeObject(model),
        Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
      };
    }
  }
}
