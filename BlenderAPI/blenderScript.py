import bpy

projectDirectory = r"C:\Users\mehme\Desktop\Blender\Blender repos\news_wall\news_wall.blend"

# Set the path to your Blender project file
blend_file_path = projectDirectory

# Open the Blender project
bpy.ops.wm.open_mainfile(filepath=blend_file_path)

# Set render settings for PNG images
bpy.context.scene.render.image_settings.file_format = 'PNG'
bpy.context.scene.render.filepath = r'C:\Users\mehme\Desktop\Blender\Python Script Output\news_wall_'
bpy.context.scene.render.image_settings.color_mode = 'RGBA'  # Optional: Set color mode to RGBA if needed

# Render the animation or frame as PNG images
bpy.ops.render.render(animation=True)

