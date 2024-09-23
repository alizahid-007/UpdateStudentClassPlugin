using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

public class UpdateStudentClassPlugin : IPlugin
{
    public void Execute(IServiceProvider serviceProvider)
    {
        IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
        if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
        {
            Entity StudentetEntity = (Entity)context.InputParameters["Target"];
            if (StudentetEntity.LogicalName == "account" && (StudentetEntity.Contains("mc_class") || StudentetEntity.Contains("mc_section")))
            {
                try
                {
                    Guid studentId = StudentetEntity.Id;
                    string fetchXml = $@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                        <entity name='mc_studentclass'>
                            <attribute name='mc_studentclassid'/>
                            <attribute name='mc_name'/>
                            <attribute name='mc_campus'/>
                            <attribute name='mc_class'/>
                            <order attribute='mc_name' descending='false'/>
                            <filter type='and'>
                                <condition attribute='statecode' operator='eq' value='0'/>
                                <condition attribute='mc_student' operator='eq' uitype='account' value='{studentId}'/>
                            </filter>
                        </entity>
                    </fetch>";

                    EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

                    if (results.Entities.Count > 0)
                    {
                        Entity studentClass = results.Entities[0];

                        if (StudentetEntity.Contains("mc_class"))
                        {
                            studentClass["mc_class"] = StudentetEntity["mc_class"];
                        }

                        if (StudentetEntity.Contains("mc_section"))
                        {
                            studentClass["mc_section"] = StudentetEntity["mc_section"];
                        }

                        service.Update(studentClass);
                    }
                    else
                    {
                        throw new InvalidPluginExecutionException("Student Class Not assigned");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("Student Class Not assigned",ex);
                }
            }
            else
            {
                throw new InvalidPluginExecutionException("Class or Section reference is null.");
            }
        }
    }
}