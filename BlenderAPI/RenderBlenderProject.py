import bpy

#projectDirectory = r"C:\Users\mehme\Desktop\Blender\Blender repos\news_wall\news_wall.blend"
projectDirectory = r"C:\Users\mehme\Desktop\Blender\Copy\news_wall\news_wall.blend"


blend_file_path = projectDirectory

bpy.ops.wm.open_mainfile(filepath=blend_file_path)

bpy.context.scene.render.image_settings.file_format = 'FFMPEG'
# bpy.context.scene.render.image_settings.file_format_args = {'codec': 'H264', 'format': 'QUICKTIME'}
bpy.context.scene.render.ffmpeg.format = 'QUICKTIME'
bpy.context.scene.render.ffmpeg.codec = 'H264'
bpy.context.scene.render.filepath = r'C:\Users\mehme\Desktop\Blender\BlenderAPI Output\news_wall.mov'

# Render the animation or frame
# bpy.ops.render.render(write_still=True)
bpy.ops.render.render(animation=True)

