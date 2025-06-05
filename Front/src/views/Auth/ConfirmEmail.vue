<template>
  <div class="confirmation-container">
    <!-- Hero Overlay Style -->
    <div class="confirmation-overlay"></div>
    
    <div class="confirmation-card">
      <!-- Left Column - Illustration -->
      <div class="confirmation-preview">
        <div class="document-container">
          <!-- Document avec image -->
          <div class="document-preview">
            <div class="document-header">
              <div class="doc-icon"><i class="bi bi-check-circle"></i></div>
              <div class="doc-title">Confirmation.pdf</div>
            </div>
            <div class="document-body">
              <div class="doc-text-line short"></div>
              <!-- Zone d'image -->
              <div class="doc-image">
                <div class="checkmark-animation" v-if="success">
                  <svg class="checkmark" viewBox="0 0 52 52">
                    <circle class="checkmark__circle" cx="26" cy="26" r="25" fill="none"/>
                    <path class="checkmark__check" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
                  </svg>
                </div>
                <div class="error-mark" v-else>
                  <svg class="error-icon" viewBox="0 0 52 52">
                    <circle class="error-icon__circle" cx="26" cy="26" r="25" fill="none"/>
                    <path class="error-icon__line" fill="none" d="M16 16 36 36 M36 16 16 36"/>
                  </svg>
                </div>
              </div>
              <div class="doc-text-line"></div>
            </div>
          </div>
        </div>

        <div class="preview-caption">
          <h3>Confirmation de compte</h3>
          <p>Votre compte est maintenant activé et sécurisé</p>
        </div>
      </div>

      <!-- Right Column - Content -->
      <div class="confirmation-content">
        <div class="content-header">
          <div class="logo-badge">
            <i class="bi bi-shield-check"></i>
          </div>
          <h2>{{ title }}</h2>
          <p class="subtitle">Processus de confirmation</p>
        </div>

        <div v-if="isLoading" class="loading-state">
          <p>Vérification en cours...</p>
          <div class="spinner"></div>
        </div>
        
        <template v-else>
          <div class="status-message" :class="{ 'success-message': success, 'error-message': !success }">
            <i :class="success ? 'bi bi-check-circle' : 'bi bi-exclamation-circle'"></i> 
            {{ message }}
          </div>
          
          <router-link 
            v-if="success" 
            to="/login" 
            class="submit-btn"
          >
            <span>Aller à la page de connexion</span>
            <i class="bi bi-arrow-right"></i>
          </router-link>
          
          <button 
            v-else 
            @click="retryConfirmation" 
            class="submit-btn"
          >
            <span>Renvoyer l'email de confirmation</span>
            <i class="bi bi-arrow-repeat"></i>
          </button>
        </template>
      </div>
    </div>
  </div>
</template>

<script>
import { useRoute } from 'vue-router';
import { ref, onMounted } from 'vue';

export default {
  setup() {
    const route = useRoute();
    const message = ref("Traitement de votre demande...");
    const title = ref("Confirmation");
    const success = ref(false);
    const isLoading = ref(false);

    onMounted(() => {
      if (route.query.success === 'true') {
        message.value = "Votre email a été confirmé avec succès !";
        title.value = "Confirmation réussie";
        success.value = true;
      } else {
        message.value = route.query.message || "Échec de la confirmation";
        title.value = "Erreur";
      }
    });

    const retryConfirmation = () => {
      console.log("Fonction de renvoi d'email");
    };

    return { message, title, success, isLoading, retryConfirmation };
  }
};
</script>

<style scoped>
.confirmation-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  background: linear-gradient(-45deg, #ffff, #4F46E5, #7884f0, #F9FAFB);
  background-size: 400% 400%;
  animation: gradient 15s ease infinite;
  position: relative;
  overflow: hidden;
}

@keyframes gradient {
  0% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
}

.confirmation-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-image: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.05'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
  opacity: 0.8;
}

.confirmation-card {
  display: flex;
  width: 100%;
  max-width: 1000px;
  min-height: 600px;
  background: white;
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
  overflow: hidden;
  z-index: 1;
}

.confirmation-preview {
  flex: 1;
  background: linear-gradient(135deg, rgba(79, 70, 229, 0.9) 0%, rgba(124, 58, 237, 0.9) 100%);
  padding: 2rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  position: relative;
}

.document-container {
  position: relative;
  width: 280px;
  height: 400px;
}

.document-preview {
  width: 100%;
  height: 100%;
  background-color: white;
  border-radius: 12px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
  overflow: hidden;
  position: relative;
  transform: perspective(1000px) rotateY(-5deg) rotateX(5deg);
}

.document-header {
  display: flex;
  align-items: center;
  padding: 12px;
  background-color: #f8f9fa;
  border-bottom: 1px solid #e9ecef;
}

.doc-icon {
  width: 28px;
  height: 28px;
  background-color: #4F46E5;
  color: white;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 10px;
  font-size: 0.9rem;
}

.doc-title {
  font-weight: 600;
  color: #333;
  font-size: 0.8rem;
}

.document-body {
  padding: 15px;
}

.doc-text-line {
  height: 10px;
  background-color: #e9ecef;
  border-radius: 4px;
  margin-bottom: 12px;
  width: 100%;
}

.doc-text-line.short {
  width: 70%;
}

.doc-image {
  height: 150px;
  background-color: #e9ecef;
  border-radius: 6px;
  margin-bottom: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
}

.checkmark-animation, .error-mark {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.checkmark {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  display: block;
  stroke-width: 4;
  stroke: #4F46E5;
  stroke-miterlimit: 10;
  animation: fill .4s ease-in-out .4s forwards, scale .3s ease-in-out .9s both;
}

.checkmark__circle {
  stroke-dasharray: 166;
  stroke-dashoffset: 166;
  stroke-width: 4;
  stroke-miterlimit: 10;
  stroke: #4F46E5;
  fill: none;
  animation: stroke .6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
}

.checkmark__check {
  transform-origin: 50% 50%;
  stroke-dasharray: 48;
  stroke-dashoffset: 48;
  animation: stroke .3s cubic-bezier(0.65, 0, 0.45, 1) .8s forwards;
}

.error-icon {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  display: block;
  stroke-width: 4;
  stroke: #EF4444;
  stroke-miterlimit: 10;
  animation: fill .4s ease-in-out .4s forwards, scale .3s ease-in-out .9s both;
}

.error-icon__circle {
  stroke-dasharray: 166;
  stroke-dashoffset: 166;
  stroke-width: 4;
  stroke-miterlimit: 10;
  stroke: #EF4444;
  fill: none;
  animation: stroke .6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
}

.error-icon__line {
  stroke-dasharray: 50;
  stroke-dashoffset: 50;
  animation: stroke .3s cubic-bezier(0.65, 0, 0.45, 1) .8s forwards;
}

@keyframes stroke {
  100% {
    stroke-dashoffset: 0;
  }
}

@keyframes scale {
  0%, 100% {
    transform: none;
  }
  50% {
    transform: scale3d(1.1, 1.1, 1);
  }
}

@keyframes fill {
  100% {
    box-shadow: inset 0 0 0 100px rgba(79, 70, 229, 0);
  }
}

.preview-caption {
  text-align: center;
  color: white;
  margin-top: 2rem;
  max-width: 300px;
}

.preview-caption h3 {
  font-size: 1.3rem;
  margin-bottom: 0.5rem;
  font-weight: 600;
}

.preview-caption p {
  font-size: 0.9rem;
  opacity: 0.9;
}

.confirmation-content {
  flex: 1;
  padding: 3rem;
  max-width: 450px;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.content-header {
  text-align: center;
  margin-bottom: 2.5rem;
}

.logo-badge {
  width: 60px;
  height: 60px;
  background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
  color: white;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.8rem;
  margin: 0 auto 1.5rem;
}

.content-header h2 {
  font-size: 1.8rem;
  color: #1F2937;
  margin-bottom: 0.5rem;
  font-weight: 700;
}

.subtitle {
  color: #6B7280;
  font-size: 0.95rem;
}

.status-message {
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.95rem;
}

.success-message {
  background-color: #DCFCE7;
  color: #166534;
}

.error-message {
  background-color: #FEE2E2;
  color: #B91C1C;
}

.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 1.5rem 0;
}

.loading-state p {
  color: #4B5563;
  font-size: 0.95rem;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #E5E7EB;
  border-top: 4px solid #4F46E5;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.submit-btn {
  width: 100%;
  padding: 1rem;
  background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%);
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  margin-top: 1rem;
}

.submit-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px -3px rgba(79, 70, 229, 0.4);
}

/* Responsive Design */
@media (max-width: 768px) {
  .confirmation-card {
    flex-direction: column;
    min-height: auto;
  }
  
  .confirmation-preview,
  .confirmation-content {
    padding: 2rem;
  }
  
  .document-container {
    width: 100%;
    max-width: 280px;
    margin: 0 auto;
  }
  
  .preview-caption {
    margin-top: 1.5rem;
  }
  
  .confirmation-content {
    max-width: none;
  }
}

@media (max-width: 480px) {
  .confirmation-container {
    padding: 1rem;
  }
  
  .confirmation-card {
    border-radius: 12px;
  }
  
  .document-preview {
    height: 300px;
  }
  
  .preview-caption h3 {
    font-size: 1.1rem;
  }
  
  .content-header h2 {
    font-size: 1.5rem;
  }
}
</style>