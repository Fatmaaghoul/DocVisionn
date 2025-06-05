import aiohttp
from ultralytics import YOLO
from deep_translator import GoogleTranslator
import tempfile
import torch
import nltk
from nltk.corpus import wordnet
from collections import defaultdict, Counter

nltk.download('wordnet')
nltk.download('omw-1.4')

translator = GoogleTranslator(source='en', target='fr')
device = 'cuda' if torch.cuda.is_available() else 'cpu'
model = YOLO('yolov8x.pt')
model.to(device)

print(f"[INFO] Utilisation du dispositif : {device}")

async def download_image_from_url(image_url: str) -> str:
    async with aiohttp.ClientSession() as session:
        async with session.get(image_url) as response:
            if response.status == 200:
                with tempfile.NamedTemporaryFile(delete=False, suffix=".png") as temp_img:
                    temp_img.write(await response.read())
                    return temp_img.name
            else:
                raise Exception(f"Erreur téléchargement image: {response.status}")

# Étape 1 : détecter les objets
def detect_objects_yolo(image_path: str) -> Counter:
    results = model(image_path, device=device)
    objects = Counter()
    for result in results:
        for box in result.boxes:
            cls_id = int(box.cls)
            label = model.names[cls_id]
            objects[label.lower()] += 1
    return objects

# Étape 2 : récupérer les synonymes traduits pour chaque objet
def get_translated_synonyms_per_object(objects: set) -> dict:
    obj_to_synonyms_fr = {}
    for obj in objects:
        synonyms = set()
        for syn in wordnet.synsets(obj, pos=wordnet.NOUN):
            if "person" in syn.lexname() or "group" in syn.lexname():
                continue
            for lemma in syn.lemmas():
                word = lemma.name().replace("_", " ").lower()
                if word.isalpha():
                    synonyms.add(word)
        synonyms.add(obj)  # inclure l’objet lui-même
        # Traduire tous les synonymes
        translated = set()
        for word in synonyms:
            try:
                translated.add(translator.translate(word).lower())
            except:
                translated.add(word)
        obj_to_synonyms_fr[obj] = translated
    return obj_to_synonyms_fr

# Étape 3 : chercher les synonymes dans le texte
def count_mentions_in_text(texts: list, obj_to_synonyms_fr: dict) -> dict:
    text_full = " ".join(texts).lower()
    mention_counts = {obj: 0 for obj in obj_to_synonyms_fr}
    for obj, synonyms in obj_to_synonyms_fr.items():
        for synonym in synonyms:
            count = text_full.count(synonym)
            mention_counts[obj] += count
            
    return mention_counts

# Étape 4 : regrouper les résultats
def count_object_occurrences(image_paths: list, texts: list) -> dict:
    result = {"result": {}}
    object_counts = defaultdict(lambda: {"occurence_text": 0, "occurence_image": 0})
    
    all_detected = Counter()
    for image_path in image_paths:
        detected = detect_objects_yolo(image_path)
        for obj, count in detected.items():
            all_detected[obj] += count
            object_counts[obj]["occurence_image"] += count
    
    translated_synonyms = get_translated_synonyms_per_object(set(all_detected.keys()))
    mention_counts = count_mentions_in_text(texts, translated_synonyms)
    
    for obj in all_detected:
        object_counts[obj]["occurence_text"] = mention_counts.get(obj, 0)
        result["result"][translator.translate(obj).lower()] = {
            "occurence_text": object_counts[obj]["occurence_text"],
            "occurence_image": object_counts[obj]["occurence_image"]
        }
        

    return result
