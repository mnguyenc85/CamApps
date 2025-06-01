import cv2
from matplotlib import pyplot as plt

def show_cv2_img(img):
  '''
  Hiển thị ảnh cv2 khi sử dụng python notebook
  '''
  # Chuyển từ BGR (OpenCV mặc định) sang RGB (cho matplotlib)
  img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)

  # Lấy kích thước ảnh
  height, width = img_rgb.shape[:2]

  # Tính figsize theo DPI mong muốn (thường 100)
  dpi = 96
  figsize = (width / dpi, height / dpi)

  # Tạo figure đúng kích thước
  plt.figure(figsize=figsize, dpi=dpi)
  # Hiển thị ảnh bằng matplotlib
  plt.imshow(img_rgb)
  plt.axis('off')  # Tắt trục
  plt.show()