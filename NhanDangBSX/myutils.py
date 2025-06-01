import numpy as np
import cv2
import os, random

class MyUtils:

    def rotate_image(image: cv2.typing.MatLike, angle: float) -> cv2.typing.MatLike:
        '''
        Quay ảnh với góc angle theo chiều ngược kim đồng hồ.
        Có mở rộng ảnh.
        Parameters:
            image: MatLike
            angle: góc quay (độ)
        '''
        # Lấy kích thước ảnh
        (h, w) = image.shape[:2]
        center = (w // 2, h // 2)  # Tâm ảnh

        # Tính kích thước canvas mới để không bị cắt góc
        abs_cos = abs(np.cos(np.radians(angle)))
        abs_sin = abs(np.sin(np.radians(angle)))
        new_w = int((h * abs_sin) + (w * abs_cos))
        new_h = int((h * abs_cos) + (w * abs_sin))

        # Tạo ma trận quay
        M = cv2.getRotationMatrix2D(center, angle, 1.0)

        # Điều chỉnh ma trận để dịch chuyển ảnh vào giữa canvas mới
        M[0, 2] += (new_w - w) / 2
        M[1, 2] += (new_h - h) / 2

        # Quay ảnh với canvas mới
        rotated = cv2.warpAffine(image, M, (new_w, new_h))

        return rotated
    
    def tach_vien(image: cv2.typing.MatLike) -> cv2.typing.MatLike:
        '''
        Tách viền bao bên ngoài khỏi ảnh (xóa về màu nền trắng). (CÓ THỂ LỖI)
        Parameters:
            image: ảnh dạng binary với nền trắng, chữ đen
        '''
        # Tìm contours
        contours, hierarchy = cv2.findContours(image, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)

        # Sắp xếp contours theo diện tích giảm dần để lấy contour lớn nhất (đường viền ngoài cùng)
        contours = sorted(contours, key=cv2.contourArea, reverse=True)

        # Lấy contour ngoài cùng (giả sử là đường bao quanh biển số)
        outer_contour = contours[0]

        mask = np.ones_like(image) * 255
        # Vẽ đường viền lên mask với màu trắng (để loại bỏ)
        cv2.drawContours(mask, [outer_contour], -1, (0), thickness=cv2.FILLED)
        #cv2.drawContours(mask, contours, -1, (127))

        # Kết hợp mask với ảnh gốc: giữ nguyên phần bên trong, xóa đường viền
        result_remove_border = cv2.bitwise_or(image, mask)

        return result_remove_border
    
    def crop_bsx(img: cv2.typing.MatLike, d, e: int) -> cv2.typing.MatLike:
        '''
        Cắt biển số xe khỏi ảnh chụp xe
        Parameters:
            img: MatLike
            d: d["box"] = [x1 y1 x2 y2]
            e: int - số điểm mở rộng
        '''
        h, w = img.shape[:2]
  
        x1, y1, x2, y2 = d["box"]
        x1, y1, x2, y2 = map(int, [x1, y1, x2, y2])

        x1 = max(0, x1 - e)
        x2 = min(w, x2 + e)
        y1 = max(0, y1 - e)
        y2 = min(h, y2 + e)

        return img[y1:y2,x1:x2]

    def load_random_image(path: str) -> cv2.typing.MatLike:
        '''
        Tải một ảnh ngẫu nhiên nằm trong thư mục
        Parameters:
            path: str - đường dẫn đến thư mục
        '''
        # Lấy danh sách tất cả các file trong thư mục
        all_files = os.listdir(path)
        # Lọc ra các file ảnh có định dạng phổ biến
        image_files = [f for f in all_files if f.lower().endswith(('.png', '.jpg', '.jpeg', '.bmp', '.tiff', '.webp'))]

        if image_files:
            # Chọn một ảnh ngẫu nhiên
            random_image = random.choice(image_files)    
            # Tạo đường dẫn đầy đủ tới ảnh
            image_path = os.path.join(path, random_image)    
            # Đọc ảnh bằng OpenCV
            return cv2.imread(image_path)