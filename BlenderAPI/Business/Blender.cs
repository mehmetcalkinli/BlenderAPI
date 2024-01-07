using BlenderAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Management.Automation;


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






        public ErrorModel RenderBlenderProject(List<string> selectedProjects)
        {

            string blenderExecutablePath = @"C:\Program Files\Blender Foundation\Blender 4.0\blender.exe";
            //"C:\Users\mehme\Desktop\Blender 4.0.lnk"
            //string blenderScriptPath = @"C:\Users\mehme\Desktop\Blender\Blender Python repos\blenderScript.py";
            string blenderScriptPath = @"C:\Users\mehme\source\repos\C# repos\BlenderAPI\BlenderAPI\blenderScript.py";


            if (selectedProjects == null || !selectedProjects.Any())
            {
                return new ErrorModel() { id=1,errorDesc= "Please provide a list of projects to render." };
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



                string script = $@"import bpy

                projectDirectory = r""C:\Users\mehme\Desktop\Blender\Blender repos\{project.projectName}\{project.projectName}.blend""

                # Set the path to your Blender project file
                blend_file_path = projectDirectory

                # Open the Blender project
                bpy.ops.wm.open_mainfile(filepath=blend_file_path)

                # Set render settings (optional)
                bpy.context.scene.render.image_settings.file_format = 'FFMPEG'
                # bpy.context.scene.render.image_settings.file_format_args = {{'codec': 'H264', 'format': 'QUICKTIME'}}
                bpy.context.scene.render.ffmpeg.format = 'QUICKTIME'
                bpy.context.scene.render.ffmpeg.codec = 'H264'
                bpy.context.scene.render.filepath = r'C:\Users\mehme\Desktop\Blender\Python Script Output\{project.projectName}.mov'

                # Render the animation or frame
                # bpy.ops.render.render(write_still=True)
                bpy.ops.render.render(animation=True)
                ";

                //                string script = $@"import bpy

                //projectDirectory = r""C:\Users\mehme\Desktop\Blender\Blender repos\{project.projectName}\{project.projectName}.blend""

                //# Set the path to your Blender project file
                //blend_file_path = projectDirectory

                //# Open the Blender project
                //bpy.ops.wm.open_mainfile(filepath=blend_file_path)

                //# Set render settings for PNG images
                //bpy.context.scene.render.image_settings.file_format = 'PNG'
                //bpy.context.scene.render.filepath = r'C:\Users\mehme\Desktop\Blender\Python Script Output\{project.projectName}_'
                //bpy.context.scene.render.image_settings.color_mode = 'RGBA'  # Optional: Set color mode to RGBA if needed

                //# Render the animation or frame as PNG images
                //bpy.ops.render.render(animation=True)
                //";


                TextWriter tw = new StreamWriter(blenderScriptPath);
                String text = script;
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

    }













}








