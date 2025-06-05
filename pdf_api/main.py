from fastapi import FastAPI, HTTPException, Body
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List, Literal, Optional, Dict
import requests
import asyncio

from objects import download_image_from_url, count_object_occurrences
from describe import describe_objects
from resume import resumer
from translate import translate_to_french
from model_manager import ModelManager
from run_model import run_model

app = FastAPI(title="Analyse Objet-Texte")

# Configuration CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # À configurer selon vos besoins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class AnalysisRequest(BaseModel):
    image_url: str
    text: str

class DescribeRequest(BaseModel):
    image_url: str
    objects: List[str]

# Initialisation du gestionnaire de modèles
model_manager = ModelManager()

# Définir un modèle par défaut au démarrage
@app.on_event("startup")
async def startup_event():
    try:
        models = model_manager.list_available_models()
        if models:
            default_model = next((m["name"] for m in models if "llava" in m["name"]), models[0]["name"])
            model_manager.set_active_model(default_model)
            print(f"Modèle par défaut défini : {default_model}")
            run_model(default_model, options={"keep_alive": -1})
            print(f"Préchargement du modèle Mistral pour le résumé")
            run_model("mistral", options={"keep_alive": -1})
    except Exception as e:
        print(f"Erreur lors de l'initialisation des modèles : {str(e)}")

@app.post("/analyze")
async def analyze(request: AnalysisRequest):
    try:
        image_path = await download_image_from_url(request.image_url)
        result = count_object_occurrences(
            image_paths=[image_path],
            texts=[request.text]
        )
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erreur lors de l'analyse : {str(e)}")

@app.post("/describe")
async def describe(request: DescribeRequest):
    """
    Génère une description des objets spécifiés dans une image.
    """
    try:
        # Validation des entrées
        if not request.image_url or not isinstance(request.image_url, str):
            raise HTTPException(status_code=400, detail="URL de l'image invalide ou manquante.")
        if not request.objects or not isinstance(request.objects, list) or not all(isinstance(obj, str) for obj in request.objects):
            raise HTTPException(status_code=400, detail="Liste d'objets invalide ou vide.")

        # Vérifier si un modèle est actif
        current_model = model_manager.get_active_model()
        if not current_model:
            raise HTTPException(status_code=400, detail="Aucun modèle actif défini. Veuillez définir un modèle via /models/set.")

        # Télécharger l'image
        try:
            image_path = await download_image_from_url(request.image_url)
        except Exception as e:
            raise HTTPException(status_code=400, detail=f"Échec du téléchargement de l'image : {str(e)}")

        # Générer la description
        description = await describe_objects(
            image_path=image_path,
            objects=request.objects,
            manager=model_manager,
            cleanup=True
        )

        # Vérifier le résultat
        if description["status"] == "error":
            raise HTTPException(status_code=500, detail=f"Erreur lors de la génération de la description : {description['message']}")

        return description

    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erreur inattendue : {type(e).__name__} - {str(e)}")
    
@app.post("/resumer")
def get_resumer(body: dict = Body(...)):
    text = body.get("text")
    if not text:
        return JSONResponse(status_code=400, content={"error": "Missing text"})
    summary = resumer(text)
    return {"summary": summary}

@app.post("/translate")
def translate(body: dict = Body(...)):
    try:
        text = body.get("text")
        if not isinstance(text, str) or not text.strip():
            raise HTTPException(status_code=400, detail="Le champ 'text' doit être une chaîne non vide")
        translated_text = translate_to_french(text)
        return {"translated_text": translated_text}
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erreur lors de la traduction : {str(e)}")

@app.get("/health")
def health_check():
    manager = ModelManager()
    return {"status": "healthy", "ollama_connection": "ok"}

@app.get("/models/available")
async def get_available_models():
    """
    Récupère la liste des modèles disponibles
    """
    try:
        models = model_manager.list_available_models()
        return {"status": "success", "models": models}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/models/current")
async def get_current_model():
    """
    Récupère le modèle actuellement actif
    """
    try:
        current_model = model_manager.get_active_model()
        if current_model is None:
            return {"status": "error", "message": "Aucun modèle n'est actuellement actif"}
        return {"status": "success", "model": current_model}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/models/set/{model_name}")
async def set_model(model_name: str):
    """
    Définit le modèle actif
    """
    try:
        result = model_manager.set_active_model(model_name)
        if result["status"] == "error":
            raise HTTPException(status_code=400, detail=result["message"])
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/models/download")
async def download_model(body: dict = Body(...)):
    """
    Démarre le téléchargement d'un modèle
    """
    model_name = body.get("model_name")
    if not model_name:
        raise HTTPException(status_code=400, detail="Le nom du modèle est requis")
    try:
        result = model_manager.download_model(model_name)
        if result["status"] == "error":
            raise HTTPException(status_code=400, detail=result["message"])
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
@app.get("/models/download-status")
async def download_status():
    status = model_manager.get_download_status()
    return status
@app.post("/models/cancel-download")
async def cancel_model_download():
    """
    Annule le téléchargement en cours
    """
    result = model_manager.cancel_download()
    return result

@app.get("/models/download-status")
async def get_download_status():
    """
    Récupère le statut du téléchargement en cours
    """
    status = model_manager.get_download_status()
    return status

@app.post("/models/run/{model_name}")
def execute_model(model_name: str):
    """
    Exécute un modèle spécifique
    """
    try:
        response_text = run_model(model_name)
        return {"status": "success", "response": response_text}
    except RuntimeError as e:
        raise HTTPException(status_code=500, detail=str(e))


