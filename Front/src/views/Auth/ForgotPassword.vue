<template>
  <div class="login-container">
    <!-- Hero Overlay Style -->
    <div class="login-overlay"></div>
    
    <div class="login-card">
      <!-- Left Column - Illustration -->
      <div class="login-preview">
        <div class="document-container">
          <!-- Document avec image -->
          <div class="document-preview">
            <div class="document-header">
              <div class="doc-icon"><i class="bi bi-file-earmark-lock"></i></div>
              <div class="doc-title">Securite_Compte.pdf</div>
            </div>
            <div class="document-body">
              <div class="doc-text-line short"></div>
              <!-- Zone d'image -->
              <div class="doc-image" :class="{ scanning: isScanning }">
                <div class="scan-overlay" v-if="isScanning"></div>
              </div>
              <div class="doc-text-line"></div>
            </div>
          </div>

          <!-- Résultats d'analyse -->
          <div class="analysis-results" v-show="showResults">
            <!-- Nuage de tags -->
            <div class="tags-cloud">
              <span v-for="(tag, index) in detectedTags" 
                    :key="index"
                    class="tag"
                    :style="tagStyle(index)">
                {{ tag }}
              </span>
            </div>
            
            <!-- Description générée -->
            <div class="image-description">
              <div class="description-text">{{ generatedDescription }}</div>
            </div>
          </div>
        </div>

        <div class="preview-caption">
          <h3>Réinitialisation sécurisée</h3>
          <p>Notre système vous enverra un lien sécurisé pour réinitialiser votre mot de passe</p>
        </div>
      </div>

      <!-- Right Column - Form -->
      <div class="login-form">
        <div class="form-header">
          <div class="logo-badge">
            <i class="bi bi-file-earmark-lock"></i>
          </div>
          <h2>Mot de passe oublié?</h2>
          <p class="subtitle">Entrez votre adresse email pour recevoir un lien de réinitialisation</p>
        </div>

        <!-- Messages d'erreur et de succès -->
        <div v-if="message && messageType === 'error'" class="error-message">
          <i class="bi bi-exclamation-circle"></i> {{ message }}
        </div>
        <div v-if="message && messageType === 'success'" class="success-message">
          <i class="bi bi-check-circle"></i> {{ message }}
        </div>

        <form @submit.prevent="sendResetEmail">
          <div class="form-group">
            <label>Email</label>
            <div class="input-with-icon">
              <i class="bi bi-envelope"></i>
              <input 
                v-model="email" 
                type="email" 
                placeholder="votre@email.com"
                class="form-input"
                required 
              />
            </div>
          </div>

          <button type="submit" class="submit-btn" :disabled="loading">
            <span>{{ loading ? 'Envoi en cours...' : 'Envoyer le lien' }}</span>
            <i class="bi bi-arrow-right"></i>
          </button>
        </form>

        <div class="divider">
          <span>ou</span>
        </div>

        <p class="signup-text">
          Vous souvenez-vous de votre mot de passe ?         
          <router-link to="/login" class="signup-link">Se connecter</router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { ref } from 'vue';

export default {
  name: 'ForgotPassword',
  setup() {
    const email = ref('');
    const loading = ref(false);
    const message = ref('');
    const messageType = ref('');
    const isScanning = ref(false);
    const showResults = ref(false);
    const detectedTags = ['Sécurité', 'Email', 'Lien', 'Réinitialisation', 'Mot de passe', 'Compte', 'Protection'];
    const generatedDescription = "Un processus sécurisé pour réinitialiser votre mot de passe et protéger votre compte.";

    const startScanAnimation = () => {
      // Cycle complet toutes les 6 secondes
      setInterval(() => {
        isScanning.value = true;
        showResults.value = false;
        
        // Après 1.5s, montrer les résultats
        setTimeout(() => {
          isScanning.value = false;
          showResults.value = true;
          
          // Après 3s, recommencer le scan
          setTimeout(() => {
            showResults.value = false;
          }, 3000);
        }, 1500);
      }, 6000);
    };

    const tagStyle = (index) => {
      const colors = ['#4F46E5', '#7C3AED', '#10B981', '#F59E0B', '#EF4444'];
      const sizes = [0.8, 0.9, 1.0, 1.1, 1.2];
      return {
        backgroundColor: colors[index % colors.length],
        fontSize: `${sizes[index % sizes.length]}rem`,
        opacity: 0,
        animation: `tag-appear 0.5s forwards ${index * 0.2}s`
      };
    };

    const sendResetEmail = async () => {
      loading.value = true;
      message.value = '';
      
      try {
        // Assurez-vous que l'URL est correcte et que le serveur est accessible
        const response = await axios.post('/api/auth/forgot-password', {
          email: email.value
        });
        
        message.value = response.data.message || response.data;
        messageType.value = 'success';
      } catch (error) {
        console.error('Error details:', error);
        message.value = error.response?.data?.message || 
                       'Une erreur réseau est survenue. Veuillez vérifier votre connexion et réessayer.';
        messageType.value = 'error';
      } finally {
        loading.value = false;
      }
    };

    return {
      email,
      loading,
      message,
      messageType,
      sendResetEmail,
      isScanning,
      showResults,
      detectedTags,
      generatedDescription,
      startScanAnimation,
      tagStyle
    };
  },
  mounted() {
    this.startScanAnimation();
  }
};
</script>

<style scoped>
.login-container {
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

.login-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-image: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.05'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
  opacity: 0.8;
}

.login-card {
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

.login-preview {
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
  background-image: url('https://images.unsplash.com/photo-1635070041078-e363dbe005cb?ixlib=rb-1.2.1&auto=format&fit=crop&w=500&q=80');
  background-size: cover;
  background-position: center;
  position: relative;
  overflow: hidden;
}

.scan-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 3px;
  background-color: rgba(79, 70, 229, 0.7);
  box-shadow: 0 0 10px 2px rgba(79, 70, 229, 0.5);
  animation: scan 1.5s linear infinite;
}

.scanning {
  animation: security-pulse 1.5s infinite;
}

@keyframes scan {
  0% { top: 0; opacity: 1; }
  100% { top: 100%; opacity: 0; }
}

@keyframes security-pulse {
  0% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.4); }
  70% { box-shadow: 0 0 0 10px rgba(16, 185, 129, 0); }
  100% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); }
}

.analysis-results {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(255, 255, 255, 0.9);
  border-radius: 12px;
  padding: 15px;
  box-sizing: border-box;
  animation: fade-in 0.5s forwards;
}

.tags-cloud {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 15px;
}

.tags-cloud .tag {
  animation: none;
}

.tag {
  display: inline-block;
  padding: 5px 12px;
  border-radius: 20px;
  color: white;
  font-size: 0.8rem;
  font-weight: 500;
}

@keyframes tag-appear {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.image-description {
  background: #f8f9fa;
  border-radius: 8px;
  padding: 12px;
  font-size: 0.85rem;
  line-height: 1.5;
  border-left: 3px solid #4F46E5;
}

.description-text {
  opacity: 0;
  animation: fade-in 0.5s forwards 1.5s;
}

@keyframes fade-in {
  from { opacity: 0; }
  to { opacity: 1; }
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

.login-form {
  flex: 1;
  padding: 3rem;
  max-width: 450px;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.form-header {
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

.logo-badge .bi-file-earmark-lock {
  font-size: 1.5rem;
}

.form-header h2 {
  font-size: 1.8rem;
  color: #1F2937;
  margin-bottom: 0.5rem;
  font-weight: 700;
}

.subtitle {
  color: #6B7280;
  font-size: 0.95rem;
}

.error-message {
  background-color: #FEE2E2;
  color: #B91C1C;
  padding: 0.8rem 1rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
}

.success-message {
  background-color: #DCFCE7;
  color: #166534;
  padding: 0.8rem 1rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #374151;
  font-size: 0.9rem;
  font-weight: 500;
}

.input-with-icon {
  position: relative;
}

.input-with-icon i {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: #9CA3AF;
  font-size: 1.1rem;
}

.form-input {
  width: 100%;
  padding: 0.875rem 1rem 0.875rem 2.8rem;
  border: 1px solid #E5E7EB;
  border-radius: 10px;
  font-size: 0.95rem;
  transition: all 0.3s ease;
  background-color: #F9FAFB;
}

.form-input:focus {
  outline: none;
  border-color: #4F46E5;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.1);
  background-color: white;
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
  margin-bottom: 1.5rem;
}

.submit-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px -3px rgba(79, 70, 229, 0.4);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.divider {
  display: flex;
  align-items: center;
  margin: 1.5rem 0;
  color: #9CA3AF;
  font-size: 0.9rem;
}

.divider::before,
.divider::after {
  content: '';
  flex: 1;
  height: 1px;
  background-color: #E5E7EB;
  margin: 0 1rem;
}

.signup-text {
  text-align: center;
  color: #6B7280;
  font-size: 0.95rem;
  margin: 0;
}

.signup-link {
  color: #4F46E5;
  text-decoration: none;
  font-weight: 600;
  transition: color 0.2s;
}

.signup-link:hover {
  color: #4338CA;
  text-decoration: underline;
}

/* Responsive Design */
@media (max-width: 768px) {
  .login-card {
    flex-direction: column;
    min-height: auto;
  }
  
  .login-preview,
  .login-form {
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
  
  .login-form {
    max-width: none;
  }
}

@media (max-width: 480px) {
  .login-container {
    padding: 1rem;
  }
  
  .login-card {
    border-radius: 12px;
  }
  
  .document-preview {
    height: 300px;
  }
  
  .preview-caption h3 {
    font-size: 1.1rem;
  }
  
  .form-header h2 {
    font-size: 1.5rem;
  }
}
</style>
