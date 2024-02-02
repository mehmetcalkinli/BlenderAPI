using BlenderAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Numerics;
using System.Web.Services.Description;
using System.Xml.Linq;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using BlenderAPI.Controllers;

namespace BlenderAPI.Business
{
    public class Blender
    {
        //private readonly IConfiguration Configuration;
        //public Blender(IConfiguration configuration) 
        //{
        //    Configuration = configuration;


        //}
        public Blender()
        {


        }
        string blenderExecutablePath = @"C:\Program Files\Blender Foundation\Blender 4.0\blender.exe";

        public List<ProjectModel> GetProjectList()
        {
            //var sDirectory = Configuration["ProjectFolder"];

            List<ProjectModel> projectList = null;


            var listD = Directory.GetDirectories("C:\\Users\\mehme\\Desktop\\Blender\\Blender repos");

            int index = 1;

            foreach (var d in listD)
            {

                var projectName = d.Split(@"\").Last();
                var projectDirectory = d;

                if (projectList == null)
                {
                    projectList = new List<ProjectModel>();
                }

                //projectList= projectList==null ? new List<ProjectModel>() : projectList;

                var project = new ProjectModel();

                project.projectName = projectName;
                project.projectDirectory = projectDirectory;
                project.id = index;

                projectList.Add(project);

                index++;
            }

            return projectList;


        }


        public (ErrorModel, List<ObjectModel>) GetObjectList(String projectName)
        {

            string blenderScriptPath = @"C:\Users\mehme\source\repos\C# repos\BlenderAPI\BlenderAPI\getPath.py";





            string script2 = $@"import bpy
import requests
import json
from mathutils import Vector
from urllib3.exceptions import InsecureRequestWarning
from bpy.types import bpy_prop_collection
requests.packages.urllib3.disable_warnings(InsecureRequestWarning)
import math

projectDirectory = r""C:\Users\mehme\Desktop\Blender\Blender repos\{projectName}\{projectName}.blend""
blend_file_path = projectDirectory
bpy.ops.wm.open_mainfile(filepath = blend_file_path)

scene = bpy.context.scene
object_info_list = []

all_objects = bpy.data.objects
def get_related_images(obj):
    images = []
    for slot in obj.material_slots:
        if slot.material:
            for node in slot.material.node_tree.nodes:
                if node.type == 'TEX_IMAGE':
                    images.append(node.image)
    return images




print(""START_JSON_OUTPUT"")

for obj in all_objects:

    object_info = {{                
        ""name"": obj.name,
        ""type"": obj.type,
        ""path"": """",
        ""projectName"": ""{projectName}"",
        ""coordinates"": {{                
            ""X"": obj.location.x,
            ""Y"": obj.location.y,
            ""Z"": obj.location.z,                             
        }},
        ""rotation"": {{
            ""Rotation_X"": int(math.degrees(obj.rotation_euler.x)),
            ""Rotation_Y"": int(math.degrees(obj.rotation_euler.y)),
            ""Rotation_Z"": int(math.degrees(obj.rotation_euler.z)),
        }}
    }}

    related_images = get_related_images(obj)  # Example function

    for image in related_images:
        object_info[""path""] = image.filepath

    if obj.type == 'FONT':
        text_data = obj.data
        object_info[""content""] = text_data.body

    object_info[""i""] = len(all_objects)

    object_info_list.append(object_info)


print(json.dumps(object_info_list, indent = 2))
 #json_string = json.dumps(object_info_list)

print(""END_JSON_OUTPUT"")
";

            TextWriter tw = new StreamWriter(blenderScriptPath);
            String text = script2;
            tw.WriteLine(text);
            tw.Close();

            // Build the PowerShell command
            string powerShellCommand = $"& \"{blenderExecutablePath}\" --background --python \"{blenderScriptPath}\"";


            // Execute the PowerShell command
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                PowerShellInstance.AddScript(powerShellCommand);
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();


                if (PowerShellInstance.HadErrors)
                {
                    string errorMessage = string.Join("\n", PowerShellInstance.Streams.Error.Select(e => e.ToString()));
                    //  return new ErrorModel() { id = 3, errorDesc = errorMessage },null;
                    return (new ErrorModel() { id = 3, errorDesc = errorMessage }, null);

                }


                string result = string.Join(Environment.NewLine, PSOutput.Select(pso => pso.ToString()));

                int startIndex = result.IndexOf('[');
                //result = result.Trim();

                //return (new ErrorModel() { id = 0, errorDesc = "Blender script execution completed successfully" }, result);

                if (startIndex >= 0)
                {
                    // Extract JSON content from the opening curly brace '{' to the end of the string
                    // string jsonContent = result.Substring(startIndex);

                    try
                    {
                        int endIndex = result.IndexOf("END_JSON_OUTPUT");
                        string jsonContent = result.Substring(startIndex, endIndex - startIndex).Trim();

                        List<ObjectModel> objectList = JsonConvert.DeserializeObject<List<ObjectModel>>(jsonContent);

                        if (BlenderController.ProjectObjectModels.ContainsKey(projectName))
                        {
                            Console.WriteLine($"Project with name '{projectName}' already exists. Objects not added.");
                            //objectList = BlenderController.ProjectObjectModels[projectName];
                            BlenderController.ProjectObjectModels[projectName] = objectList;
                            return (new ErrorModel() { id = 0, errorDesc = "Blender script execution completed successfully" }, objectList);
                        }

                        BlenderAPI.Controllers.BlenderController.ProjectObjectModels.Add(projectName, objectList);

                        return (new ErrorModel() { id = 0, errorDesc = "Blender script execution completed successfully" }, objectList);
                    }
                    catch (Exception ex)
                    {
                        return (new ErrorModel() { id = 3, errorDesc = $"Error parsing Blender script output: {ex.Message}" }, null);
                    }
                }
                else
                {
                    return (new ErrorModel() { id = 3, errorDesc = "No JSON content found in Blender script output" }, null);
                }



            }
            //return new ErrorModel() { id = 0, errorDesc = "Blender script execution completed successfully" },result;



        }



        public void SaveObject(List<ObjectModel> objectModelList)
        {

            string editedObjects = "";
            int count = 0;
           
            
            string blenderScriptPath = @"C:\Users\mehme\source\repos\C# repos\BlenderAPI\BlenderAPI\SaveObject.py";



            objectModelList.ForEach(obj => {

                string tempStr = "";

                if (obj.type == "FONT")
                {
                    tempStr = $@"{{""text_object_name"": ""{obj.name}"", ""new_text_content"": r""{obj.content}""}}";
                }
                else if (obj.type == "MESH")
                {
                    tempStr = $@"{{""text_object_name"": ""{obj.name}"", ""new_text_content"": r""{obj.path}""}}";
                }


                count++;

                if (count == objectModelList.Count)
                {
                    editedObjects += tempStr;
                }
                else
                {
                    editedObjects += tempStr + ",";
                }
            });
            ObjectModel objectModel = objectModelList[0];//Get the first object to understand which project these objects belong to



            string scriptImage = $@"import bpy
import json
import os

projectDirectory = r""C:\Users\mehme\Desktop\Blender\Blender repos\{objectModel.projectName}\{objectModel.projectName}.blend""
blend_file_path = projectDirectory
bpy.ops.wm.open_mainfile(filepath = blend_file_path)

all_objects = bpy.data.objects

def update_text_content(text_object, new_text_content):
    # Access the text data
    text_data = text_object.data

    original_text_content[text_object.name] = text_data.body

    # Edit the content of the text data
    text_data.body = new_text_content

    print(f""Text content for object '{{text_object.name}}' updated successfully."")


def get_related_images(obj):
    images = []
    for slot in obj.material_slots:
        if slot.material:
            for node in slot.material.node_tree.nodes:
                if node.type == 'TEX_IMAGE':
                    images.append(node.image)
    return images



api_response = [{editedObjects}]

original_text_content = {{}}

for api_data in api_response:
    text_object = bpy.data.objects.get(api_data[""text_object_name""])

    if text_object and text_object.type == 'FONT':
        # Store the original text content for potential rollback
        original_text_content[text_object.name] = text_object.data.body

        update_text_content(text_object, api_data[""new_text_content""])
        print(f""Updated text content for object '{{text_object.name}}': {{api_data['new_text_content']}}"")

    elif text_object and text_object.type == 'MESH':

        related_images = get_related_images(text_object)  # Example function

        for image in related_images:
            image.filepath=api_data[""new_text_content""]




    else:
        print(f""Text object '{{api_data['text_object_name']}}' not found or is not a text object."")


output_folder = r""C:\Users\mehme\Desktop\Blender\Copy\{objectModel.projectName}""
os.makedirs(output_folder, exist_ok=True)

output_file_path = os.path.join(output_folder, ""{objectModel.projectName}.blend"")

#output_file_path = r""C:\Users\mehme\Desktop\Blender\Copy\{objectModel.projectName}\{objectModel.projectName}.blend""

if os.path.exists(output_file_path):
    os.remove(output_file_path)
    print(f""Deleted existing file: {{output_file_path}}"")

bpy.ops.wm.save_mainfile(filepath=output_file_path)
print(f""New project saved as: {{output_file_path}}"")

for object_name, original_content in original_text_content.items():
    text_object = bpy.data.objects.get(object_name)
    if text_object and text_object.type == 'FONT':
        update_text_content(text_object, original_content)
        print(f""Reverted text content for object '{{text_object.name}}' to: {{original_content}}"")
";


            ExecuteScript(scriptImage, blenderScriptPath);
        }












        public ErrorModel RenderBlenderProject(List<string> selectedProjects)
        {

            string blenderScriptPath = @"C:\Users\mehme\source\repos\C# repos\BlenderAPI\BlenderAPI\RenderBlenderProject.py";


            if (selectedProjects == null || !selectedProjects.Any())
            {
                return new ErrorModel() { id = 1, errorDesc = "Please provide a list of projects to render." };
            }




            Blender blender = new Blender();

            List<ProjectModel> allProjects = blender.GetProjectList();

            List<ProjectModel> validProjects = allProjects.Where(project => selectedProjects.Contains(project.projectName)).ToList();

            if (validProjects.Count == 0)
            {
                return new ErrorModel() { id = 2, errorDesc = "No valid projects selected for rendering." };
            }





            foreach (var project in validProjects)
            {



                string scriptRender = $@"import bpy

#projectDirectory = r""C:\Users\mehme\Desktop\Blender\Blender repos\{project.projectName}\{project.projectName}.blend""
projectDirectory = r""C:\Users\mehme\Desktop\Blender\Copy\{project.projectName}\{project.projectName}.blend""


blend_file_path = projectDirectory

bpy.ops.wm.open_mainfile(filepath=blend_file_path)

bpy.context.scene.render.image_settings.file_format = 'FFMPEG'
# bpy.context.scene.render.image_settings.file_format_args = {{'codec': 'H264', 'format': 'QUICKTIME'}}
bpy.context.scene.render.ffmpeg.format = 'QUICKTIME'
bpy.context.scene.render.ffmpeg.codec = 'H264'
bpy.context.scene.render.filepath = r'C:\Users\mehme\Desktop\Blender\BlenderAPI Output\{project.projectName}.mov'

# Render the animation or frame
# bpy.ops.render.render(write_still=True)
bpy.ops.render.render(animation=True)
";


                //Script to render projects as png

                //                string script = $@"import bpy

                //projectDirectory = r""C:\Users\mehme\Desktop\Blender\Blender repos\{project.projectName}\{project.projectName}.blend""

                //# Set the path to your Blender project file
                //blend_file_path = projectDirectory

                //# Open the Blender project
                //bpy.ops.wm.open_mainfile(filepath=blend_file_path)

                //# Set render settings for PNG images
                //bpy.context.scene.render.image_settings.file_format = 'PNG'
                //bpy.context.scene.render.filepath = r'C:\Users\mehme\Desktop\Blender\BlenderAPI Output\{project.projectName}\{project.projectName}_'
                //bpy.context.scene.render.image_settings.color_mode = 'RGBA'  # Optional: Set color mode to RGBA if needed

                //# Render the animation or frame as PNG images
                //bpy.ops.render.render(animation=True)
                //";


                TextWriter tw = new StreamWriter(blenderScriptPath);
                String text = scriptRender;
                tw.WriteLine(text);
                tw.Close();








                // Build the PowerShell command
                string powerShellCommand = $"& \"{blenderExecutablePath}\" --background --python \"{blenderScriptPath}\"";

                // Execute the PowerShell command
                using (PowerShell PowerShellInstance = PowerShell.Create())
                {
                    PowerShellInstance.AddScript(powerShellCommand);
                    Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                    // Check for errors
                    if (PowerShellInstance.HadErrors)
                    {
                        string errorMessage = string.Join("\n", PowerShellInstance.Streams.Error.Select(e => e.ToString()));
                        return new ErrorModel() { id = 3, errorDesc = errorMessage };
                    }


                }

            };
            return new ErrorModel() { id = 0, errorDesc = "Blender script execution completed successfully" };
        }



       



        public void ExecuteScript(string script, string blenderScriptPath)
        {


            TextWriter tw = new StreamWriter(blenderScriptPath);
            string text = script;
            tw.WriteLine(text);
            tw.Close();

            // Build the PowerShell command
            string powerShellCommand = $"& \"{blenderExecutablePath}\" --background --python \"{blenderScriptPath}\"";


            // Execute the PowerShell command
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                PowerShellInstance.AddScript(powerShellCommand);
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();


                if (PowerShellInstance.HadErrors)
                {
                    string errorMessage = string.Join("\n", PowerShellInstance.Streams.Error.Select(e => e.ToString()));

                }

            }

        }
        
        
    }



}








