import bpy
import os
import json
from pathlib import Path
import tkinter as tk
from tkinter import filedialog

root = tk.Tk()
root.withdraw()

fbxFile = Path(__file__).parent.parent /"raw_dictionary"/"clara_RU_salada_067_Motion.Fbx"
file_path = filedialog.askopenfilename(
    title="Select a file",
    filetypes=(("fbx files", "*.fbx"), ("Fbx Files", "*.Fbx"))
)
fbxFileName = os.path.basename(file_path)

bpy.ops.import_scene.fbx(filepath=str(file_path))
armature = bpy.data.objects["Armature"]
bones = armature.pose.bones

armature.rotation_mode = "QUATERNION"

scene = bpy.context.scene

##### frame counting #######
frame_start = 148  
frame_end = 202
name = "LARANJA"

total_frames = frame_end - frame_start + 1

##### getting quaternion data from bones in animation (order x, y, z, w) #######
quaternion_data = {
    "FrameCount": total_frames,
}

for frame in range(frame_start, frame_end + 1):
    scene.frame_set(frame)
    # print(f"Frame {frame}:")

    frame_data = {
      "FrameId": frame + 1
    }

    for b in bones:
        
        q = b.rotation_quaternion.copy()
        # print(f"    Bone {b.basename}: \n {q}")

        frame_data[b.basename] = [q.x, q.y, q.z, q.w]

    quaternion_data.setdefault("FrameData", []).append(frame_data)

# print(quaternion_data)
# for frame_data in quaternion_data["FrameData"]:
#     for key, value in frame_data.items():
#         print(f"{key}: {value}")

##### putting it into json files ######
json_filename = '/'.join([os.path.dirname(os.path.dirname(fbxFile)) ,"json_dictionary", f"{name}_1.json"])
print(json_filename)

counter = 1
while os.path.exists(json_filename):
    json_filename = json_filename.replace(f"{counter}.json", f"{counter + 1}.json")
    counter += 1

with open(json_filename, 'w') as f:
    json.dump(quaternion_data, f, indent=4)
