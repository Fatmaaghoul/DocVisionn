import fitz  # PyMuPDF
import requests
import tempfile
import cloudinary
import cloudinary.uploader
import os
from dotenv import load_dotenv

load_dotenv()

cloudinary.config(
    cloud_name=os.getenv("CLOUDINARY_CLOUD_NAME"),
    api_key=os.getenv("CLOUDINARY_API_KEY"),
    api_secret=os.getenv("CLOUDINARY_API_SECRET")
)

def download_pdf_from_url(url: str) -> str:
    response = requests.get(url)
    if response.status_code == 200:
        temp = tempfile.NamedTemporaryFile(delete=False, suffix=".pdf")
        temp.write(response.content)
        temp.close()
        return temp.name
    else:
        raise Exception(f"Unable to download PDF. Status code: {response.status_code}")

def extract_images_and_text(pdf_path: str):
    doc = fitz.open(pdf_path)
    text = ""
    image_urls = []

    for page_num in range(len(doc)):
        page = doc.load_page(page_num)
        text += page.get_text()

        image_list = page.get_images(full=True)
        for img in image_list:
            xref = img[0]
            base_image = doc.extract_image(xref)
            image_bytes = base_image["image"]

            with tempfile.NamedTemporaryFile(delete=False, suffix=".png") as temp_img:
                temp_img.write(image_bytes)
                temp_img.flush()

                # Upload image to Cloudinary
                result = cloudinary.uploader.upload(temp_img.name, folder="pdf_images")
                image_urls.append(result["secure_url"])

            os.remove(temp_img.name)

    return text, image_urls
