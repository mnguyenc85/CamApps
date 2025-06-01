from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.responses import JSONResponse
from contextlib import asynccontextmanager
from PIL import Image
from io import BytesIO
import torch

models = {}

@asynccontextmanager
async def lifespan(app: FastAPI):
    # 👉 Khởi tạo tài nguyên khi app bắt đầu
    models["bsx"] = torch.hub.load('ultralytics/yolov5', 'custom', path='bsx_kalyan1045.pt')
    print("Model loaded.")
    yield
    # 👉 Dọn dẹp tài nguyên khi app shutdown
    print("Shutting down...")

app = FastAPI(lifespan=lifespan)

@app.post("/detect/")
async def detect_objects(file: UploadFile = File(...)):
    try:
        # Đọc ảnh
        contents = await file.read()
        image = Image.open(BytesIO(contents)).convert("RGB")

        # Inference
        model = models["bsx"]
        results = model(image)

        # Chuyển kết quả thành JSON
        detections = results.pandas().xyxy[0].to_dict(orient="records")

        return JSONResponse(content={
            "filename": file.filename,
            "detections": detections
        })

    except Exception as e:
        raise HTTPException(status_code=400, detail=f"Image processing failed: {e}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=False)