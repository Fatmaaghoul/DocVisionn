import aiohttp
from ultralytics import YOLO
from deep_translator import GoogleTranslator
import tempfile
import os
import nltk
from nltk.corpus import wordnet

device = 'cpu'

# Initialisation du traducteur
translator = GoogleTranslator(source='en', target='fr')

# Chargement du modèle YOLO avec le dispositif spécifié
model = YOLO('yolov11x.pt')
model.to(device) 
print(f"[INFO] Modèle YOLO chargé sur : {device}")

# Fonction pour télécharger une image depuis une URL
async def download_image_from_url(image_url: str) -> str:
    async with aiohttp.ClientSession() as session:
        async with session.get(image_url) as response:
            if response.status == 200:
                # Enregistrer l'image temporairement
                with tempfile.NamedTemporaryFile(delete=False, suffix=".png") as temp_img:
                    temp_img.write(await response.read())
                    temp_img_path = temp_img.name
                print(f"[INFO] Image téléchargée et enregistrée à : {temp_img_path}")
                return temp_img_path
            else:
                raise Exception(f"Unable to download image. Status code: {response.status}")

# Détecter les objets avec YOLO
def detect_objects_yolo(image_path: str) -> set:
    # Effectuer la détection avec YOLO sur le dispositif spécifié
    results = model(image_path, device=device)
    detected_objects = set()
    for result in results:
        for box in result.boxes:
            cls_id = int(box.cls)
            label = model.names[cls_id]
            detected_objects.add(label.lower())
    print(f"[INFO] Objets détectés par YOLO : {detected_objects}")
    return detected_objects

# Traduire les objets en français
def translate_objects_to_french(objects: set) -> set:
    translated_objects = set()
    for obj in objects:
        try:
            translated = translator.translate(obj).lower()
            translated_objects.add(translated)
        except Exception as e:
            print(f"[WARNING] Erreur de traduction pour '{obj}' : {e}")
            translated_objects.add(obj)  # Garder l'original en cas d'erreur
    print(f"[INFO] Objets traduits en français : {translated_objects}")
    return translated_objects

# Récupérer les synonymes des objets détectés et les traduire
def get_synonyms(objects: set) -> set:
    synonyms = set()
    for obj in objects:
        # Récupérer les synonymes via WordNet, limités aux noms (objets physiques)
        for syn in wordnet.synsets(obj, pos=wordnet.NOUN, lang='eng'):
            for lemma in syn.lemmas():
                synonym = lemma.name().lower().replace('_', ' ')
                synonyms.add(synonym)
        # Ajouter l'objet original
        synonyms.add(obj)
    print(f"[INFO] Synonymes en anglais : {synonyms}")
    
    # Traduire les objets et synonymes en français
    translated_synonyms = translate_objects_to_french(synonyms)
    print(f"[INFO] Synonymes traduits en français : {translated_synonyms}")
    
    return translated_synonyms

# Extraire les objets mentionnés dans le texte
def extract_mentioned_objects(text: str, reference_objects: set) -> set:
    text = text.lower()
    mentioned_objects = set()
    
    # Parcourir le texte pour trouver des correspondances avec les objets de référence
    for obj in reference_objects:
        if obj in text:
            mentioned_objects.add(obj)
    
    print(f"[INFO] Objets mentionnés dans le texte : {mentioned_objects}")
    return mentioned_objects
