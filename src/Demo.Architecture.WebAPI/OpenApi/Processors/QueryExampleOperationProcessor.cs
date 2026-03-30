using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Demo.Architecture.WebAPI.OpenApi.Processors;

public class QueryExampleOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        foreach (var parameter in context.OperationDescription.Operation.Parameters)
        {
            switch (parameter.Name)
            {
                case "page":
                    parameter.Example = 1;
                    break;

                case "pageSize":
                    parameter.Example = 20;
                    break;

                case "searchTerm":
                    parameter.Example = "example search";
                    break;

                case "sort":
                    parameter.Example = "name_desc";
                    break;
            }
        }

        return true;
    }
}
