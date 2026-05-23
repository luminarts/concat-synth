import bpy
import os
import json
from pathlib import Path

fbxFile = Path(__file__).parent.parent /"raw_dictionary"/"clara_RU_salada_067_Motion.Fbx"
fbxFileName = os.path.basename(fbxFile)

bpy.ops.import_scene.fbx(filepath=str(fbxFile))
armature = bpy.data.objects["Armature"]
bones = armature.pose.bones

armature.rotation_mode = "QUATERNION"

scene = bpy.context.scene

##### frame counting #######
frame_start = scene.frame_start
frame_end = scene.frame_end

total_frames = frame_end - frame_start + 1

##### getting quaternion data from bones in animation (order x, y, z, w) #######
quaternion_data = {
    "FrameCount": total_frames,
}

for frame in range(total_frames):
    scene.frame_set(frame)
    print(f"Frame {frame}:")

    frame_data = {
      "FrameId": frame + 1
    }

    for b in bones:
        
        q = b.rotation_quaternion.copy()
        print(f"    Bone {b.basename}: \n {q}")

        frame_data[b.basename] = [q.x, q.y, q.z, q.w]

    quaternion_data.setdefault("FrameData", []).append(frame_data)

print(quaternion_data)
for frame_data in quaternion_data["FrameData"]:
    for key, value in frame_data.items():
        print(f"{key}: {value}")

##### putting it into json files ######
json_filename = '/'.join([os.path.dirname(os.path.dirname(fbxFile)) ,"json_dictionary", f"{os.path.splitext(fbxFileName)[0]}.json"])
print(json_filename)

with open(json_filename, 'w') as f:
    json.dump(quaternion_data, f, indent=4)
