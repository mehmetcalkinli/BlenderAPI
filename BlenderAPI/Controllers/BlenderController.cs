using BlenderAPI.Business;
using BlenderAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.ObjectModel;
using Microsoft.PowerShell;
using System.Management.Automation;
using System.Linq;
using System;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;
using Newtonsoft.Json;

namespace BlenderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlenderController : ControllerBase
    {
        //private static readonly List<ObjectModel> ObjectModelsList = new List<ObjectModel>();
        public static readonly Dictionary<string, List<ObjectModel>> ProjectObjectModels = new Dictionary<string, List<ObjectModel>>();
        


        [HttpPost]
        [Route("GetList")]

        public List<ProjectListModel> GetList()
        {

            Blender blender = new Blender();

            List<ProjectListModel> list = new List<ProjectListModel>();


            var projectList=blender.GetProjectList();


            foreach (var proj in projectList) 
            {

                ProjectListModel projectListModel = new ProjectListModel();

                projectListModel.projectModel = proj;

                (ErrorModel error, List<ObjectModel> objectList) = blender.GetObjectList(proj.projectName);

                projectListModel.objectModel = objectList;

                list.Add(projectListModel);
            }

            return list;
        }





        [HttpPost]
        [Route("SaveObject")]

        public void SaveObject(List<ObjectModel> objectModelList)
        {

            Blender blender = new Blender();    

            blender.SaveObject(objectModelList);

        }





        [HttpPost]
        [Route("RenderBlenderProject")]
        public IActionResult RenderBlenderProject([FromBody] List<string> selectedProjects)
        {

            Blender blender = new Blender();

            ErrorModel error = blender.RenderBlenderProject(selectedProjects);

            if (error.id != 0)
            {
                return BadRequest(new { success = false, message = error.errorDesc });

            }

            return Ok(new { success = true, message = error.errorDesc });




        }
    }
}
