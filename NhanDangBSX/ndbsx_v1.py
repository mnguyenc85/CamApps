import torch

class NDBSXv1:
    def __init__(self, device='cpu'):
        self.model = torch.hub.load('ultralytics/yolov5', 'custom', path='models/bsx_kalyan1045.pt', device=device)

    def find_bsx_rect(self, img):
        boxes = self.model(img)

        results = []
        for *box, conf, class_id in boxes.xyxy[0]:
            results.append({"box": [t.item() for t in box], "conf": conf.item(), "cid": class_id.item(), "lp": "" })

        return results