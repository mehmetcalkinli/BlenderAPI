import bpy
import requests
import json
from mathutils import Vector
from urllib3.exceptions import InsecureRequestWarning
from bpy.types import bpy_prop_collection
requests.packages.urllib3.disable_warnings(InsecureRequestWarning)
import math

projectDirectory = r"C:\Users\mehme\Desktop\Blender\Blender repos\news_wall - 2\news_wall - 2.blend"
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




print("START_JSON_OUTPUT")

for obj in all_objects:

    object_info = {                
        "name": obj.name,
        "type": obj.type,
        "path": "",
        "projectName": "news_wall - 2",
        "coordinates": {                
            "X": obj.location.x,
            "Y": obj.location.y,
            "Z": obj.location.z,                             
        },
        "rotation": {
            "Rotation_X": int(math.degrees(obj.rotation_euler.x)),
            "Rotation_Y": int(math.degrees(obj.rotation_euler.y)),
            "Rotation_Z": int(math.degrees(obj.rotation_euler.z)),
        }
    }

    related_images = get_related_images(obj)  # Example function

    for image in related_images:
        object_info["path"] = image.filepath

    if obj.type == 'FONT':
        text_data = obj.data
        object_info["content"] = text_data.body

    object_info["i"] = len(all_objects)

    object_info_list.append(object_info)


print(json.dumps(object_info_list, indent = 2))
 #json_string = json.dumps(object_info_list)

print("END_JSON_OUTPUT")

