# Azure DataLake Gen2 Helper
![Build status](https://dev.azure.com/peterdowdy/Adlg2Helper/_apis/build/status/Adlg2Helper%20Release)

This is a library wrapping calls to Azure DataLake Gen2's REST API (docs [here](https://docs.microsoft.com/en-us/rest/api/storageservices/data-lake-storage-gen2)).

This first release is just a helper that makes it easier to make correct calls to the API. Later releases will provide helpers to assist with more complex tasks, such as automatically segmenting and uploading large files.

This library depends on Newtonsoft.Json becuase all libraries do, and on Polly to provide retry semantics.

## Getting started
This release supports shared key authentication or oauth.. Install in your application like this:
```
var host = new HostBuilder()
    .ConfigureServices(svc =>
        {
            svc.AddAzureDataLakeGen2Client(options =>
            {
                options.AuthorizeWithAccountNameAndKey("account name", "shared key");
                // or //
                options.AuthorizeWithAccountNameAndAzureOauth("account name", "tenant id", "client id", "client secret");
            });
        })
    .Build();
//later in your code
var pathClient = services.GetRequiredService<Adlg2PathClient>();
var filesystemClient = services.GetRequiredService<Adlg2FilesystemClient>();
```

Or create it directly using the factory:
```
var pathClient = Adlg2ClientFactory.BuildPathClient("account name", "shared key");
// or //
var pathClient = Adlg2ClientFactory.BuildPathClient("account name", "tenant id", "client id", "client secret");

var filesystemClient = Adlg2ClientFactory.BuildFilesystemClient("account name", "shared key");
// or //
var filesystemClient = Adlg2ClientFactory.BuildFilesystemClient("account name", "tenant id", "client id", "client secret");
```

### Path client
The path client wraps the [path API](https://docs.microsoft.com/en-us/rest/api/storageservices/datalakestoragegen2/path).

### Filesystem client
The filesystem client wraps the [filesystem API](https://docs.microsoft.com/en-us/rest/api/storageservices/datalakestoragegen2/filesystem).

#### Finally
Let me know if this was useful for you or if you have suggestions or feature requests (issues welcome). The current roadmap is:
* Add SAS support.
* Allow configuration of retry policies.
* Create helpers to segment upload and download of large files. 
