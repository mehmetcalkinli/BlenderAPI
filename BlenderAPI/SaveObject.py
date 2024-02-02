import bpy
import json
import os

projectDirectory = r"C:\Users\mehme\Desktop\Blender\Blender repos\news_wall\news_wall.blend"
blend_file_path = projectDirectory
bpy.ops.wm.open_mainfile(filepath = blend_file_path)

all_objects = bpy.data.objects

def update_text_content(text_object, new_text_content):
    # Access the text data
    text_data = text_object.data

    original_text_content[text_object.name] = text_data.body

    # Edit the content of the text data
    text_data.body = new_text_content

    print(f"Text content for object '{text_object.name}' updated successfully.")


def get_related_images(obj):
    images = []
    for slot in obj.material_slots:
        if slot.material:
            for node in slot.material.node_tree.nodes:
                if node.type == 'TEX_IMAGE':
                    images.append(node.image)
    return images



api_response = [{"text_object_name": "Text", "new_text_content": r"ASDADSTT"},{"text_object_name": "Dottod World Map", "new_text_content": r"C:\Users\mehme\Desktop\Blender\white-dots-world-map-vector.jpg"}]

original_text_content = {}

for api_data in api_response:
    text_object = bpy.data.objects.get(api_data["text_object_name"])

    if text_object and text_object.type == 'FONT':
        # Store the original text content for potential rollback
        original_text_content[text_object.name] = text_object.data.body

        update_text_content(text_object, api_data["new_text_content"])
        print(f"Updated text content for object '{text_object.name}': {api_data['new_text_content']}")

    elif text_object and text_object.type == 'MESH':

        related_images = get_related_images(text_object)  # Example function

        for image in related_images:
            image.filepath=api_data["new_text_content"]




    else:
        print(f"Text object '{api_data['text_object_name']}' not found or is not a text object.")


output_folder = r"C:\Users\mehme\Desktop\Blender\Copy\news_wall"
os.makedirs(output_folder, exist_ok=True)

output_file_path = os.path.join(output_folder, "news_wall.blend")

#output_file_path = r"C:\Users\mehme\Desktop\Blender\Copy\news_wall\news_wall.blend"

if os.path.exists(output_file_path):
    os.remove(output_file_path)
    print(f"Deleted existing file: {output_file_path}")

bpy.ops.wm.save_mainfile(filepath=output_file_path)
print(f"New project saved as: {output_file_path}")

for object_name, original_content in original_text_content.items():
    text_object = bpy.data.objects.get(object_name)
    if text_object and text_object.type == 'FONT':
        update_text_content(text_object, original_content)
        print(f"Reverted text content for object '{text_object.name}' to: {original_content}")

