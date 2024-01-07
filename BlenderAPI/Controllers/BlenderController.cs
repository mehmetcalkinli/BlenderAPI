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

namespace BlenderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlenderController : ControllerBase
    {
        [HttpGet]
        public List<ProjectModel> GetProjectList()
        {
            Blender blender = new Blender();




            return blender.GetProjectList();
        }





        [HttpPost]
        [Route("render")]
        public IActionResult RenderBlenderProject([FromBody] List<string> selectedProjects)
        {

            Blender blender = new Blender();

            ErrorModel error = blender.RenderBlenderProject(selectedProjects);

            if (error.id!=0)
            {
                return BadRequest(new { success = false, message = error.errorDesc });

            }            
            
            return Ok(new { success = true, message = error.errorDesc });

            


        }







       
    }
}
