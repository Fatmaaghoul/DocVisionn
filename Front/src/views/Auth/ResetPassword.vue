<template>
  <div class="reset-container">
    <!-- Hero Overlay Style -->
    <div class="reset-overlay"></div>
    
    <div class="reset-card">
      <!-- Left Column - Illustration -->
      <div class="reset-preview">
        <div class="document-container">
          <!-- Document avec image -->
          <div class="document-preview">
            <div class="document-header">
              <div class="doc-icon"><i class="bi bi-shield-lock"></i></div>
              <div class="doc-title">Password_Reset.pdf</div>
            </div>
            <div class="document-body">
              <div class="doc-text-line short"></div>
              <!-- Zone d'image -->
              <div class="doc-image">
                <div class="lock-animation">
                  <svg viewBox="0 0 24 24" width="80" height="80" fill="none" stroke="#4F46E5" stroke-width="2">
                    <path d="M5 13V8a7 7 0 0114 0v5"></path>
                    <rect x="3" y="13" width="18" height="10" rx="2"></rect>
                    <circle cx="12" cy="18" r="1"></circle>
                  </svg>
                </div>
              </div>
              <div class="doc-text-line"></div>
              <div class="doc-text-line short"></div>
            </div>
          </div>
        </div>

        <div class="preview-caption">
          <h3>Sécurité renforcée</h3>
          <p>Définissez un nouveau mot de passe sécurisé pour protéger votre compte</p>
        </div>
      </div>

      <!-- Right Column - Form -->
      <div class="reset-form">
        <div class="form-header">
          <div class="logo-badge">
            <i class="bi bi-shield-lock"></i>
          </div>
          <h2>Réinitialisation</h2>
          <p class="subtitle">Définissez un nouveau mot de passe sécurisé</p>
        </div>

        <div v-if="message" :class="['status-message', status]">
          <i :class="status === 'success' ? 'bi bi-check-circle' : 'bi bi-exclamation-circle'"></i> 
          {{ message }}
        </div>

        <form @submit.prevent="resetPassword">
          <div class="form-group">
            <label>Nouveau mot de passe</label>
            <div class="input-with-icon">
              <i class="bi bi-lock"></i>
              <input 
                v-model="newPassword" 
                :type="showPassword ? 'text' : 'password'"
                placeholder="Votre nouveau mot de passe"
                class="form-input"
                required 
              />
              <button 
                type="button" 
                class="toggle-password"
                @click="showPassword = !showPassword"
              >
                <i class="bi" :class="showPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
              </button>
            </div>
          </div>
          
          <div class="form-group">
            <label>Confirmer le mot de passe</label>
            <div class="input-with-icon">
              <i class="bi bi-lock-fill"></i>
              <input 
                v-model="confirmPassword" 
                :type="showConfirmPassword ? 'text' : 'password'"
                placeholder="Confirmez votre mot de passe"
                class="form-input"
                required 
              />
              <button 
                type="button" 
                class="toggle-password"
                @click="showConfirmPassword = !showConfirmPassword"
              >
                <i class="bi" :class="showConfirmPassword ? 'bi-eye-slash' : 'bi-eye'"></i>
              </button>
            </div>
          </div>

          <button type="submit" class="submit-btn" :disabled="loading">
            <span>{{ loading ? 'Enregistrement...' : 'Réinitialiser' }}</span>
            <i class="bi" :class="loading ? 'bi-arrow-repeat' : 'bi-arrow-right'"></i>
          </button>
        </form>

        <div class="security-tips">
          <h4>Conseils de sécurité :</h4>
          <ul>
            <li><i class="bi bi-check-circle"></i> Utilisez au moins 8 caractères</li>
            <li><i class="bi bi-check-circle"></i> Combinez lettres, chiffres et symboles</li>
            <li><i class="bi bi-check-circle"></i> Évitez les mots de passe courants</li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';

export default {
  setup() {
    const newPassword = ref('');
    const confirmPassword = ref('');
    const message = ref('');
    const status = ref('');
    const loading = ref(false);
    const showPassword = ref(false);
    const showConfirmPassword = ref(false);
    const route = useRoute();
    const router = useRouter();
    const token = ref('');
    const email = ref('');

    onMounted(() => {
      // Récupérer et nettoyer le token
      const rawToken = route.query.token || '';
      token.value = decodeURIComponent(rawToken).replace(/ /g, '+');
      email.value = route.query.email || '';
      
      if (!token.value || !email.value) {
        message.value = "Token ou email manquant dans l'URL";
        status.value = 'error';
      }
    });

    const resetPassword = async () => {
      if (newPassword.value !== confirmPassword.value) {
        message.value = "Les mots de passe ne correspondent pas";
        status.value = 'error';
        return;
      }

      if (!token.value || !email.value) {
        message.value = "Token ou email manquant";
        status.value = 'error';
        return;
      }

      loading.value = true;
      try {
        const response = await axios.post('/api/auth/reset-password', {
          email: email.value,
          token: token.value,
          newPassword: newPassword.value
        });

        message.value = "Mot de passe réinitialisé avec succès";
        status.value = 'success';
        setTimeout(() => {
          router.push('/login');
        }, 3000);
      } catch (error) {
        message.value = error.response?.data?.message || error.response?.data || "Une erreur est survenue";
        status.value = 'error';
      } finally {
        loading.value = false;
      }
    };

    return { 
      newPassword,
      confirmPassword,
      message,
      resetPassword,
      status,
      loading,
      showPassword,
      showConfirmPassword
    };
  }
};
</script>

<style scoped>
.reset-container {
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

.reset-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-image: url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='0.05'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E");
  opacity: 0.8;
}

.reset-card {
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

.reset-preview {
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

.lock-animation {
  animation: bounce 2s infinite;
}

@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
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

.reset-form {
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

.status-message {
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.95rem;
}

.status-message.success {
  background-color: #DCFCE7;
  color: #166534;
}

.status-message.error {
  background-color: #FEE2E2;
  color: #B91C1C;
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

.toggle-password {
  position: absolute;
  right: 1rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #9CA3AF;
  cursor: pointer;
  padding: 0;
  font-size: 1.1rem;
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

.submit-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 15px -3px rgba(79, 70, 229, 0.4);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.security-tips {
  margin-top: 2rem;
  padding: 1rem;
  background-color: #F9FAFB;
  border-radius: 10px;
  border-left: 4px solid #4F46E5;
}

.security-tips h4 {
  color: #1F2937;
  font-size: 1rem;
  margin-bottom: 0.5rem;
}

.security-tips ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.security-tips li {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #4B5563;
  font-size: 0.9rem;
  margin-bottom: 0.5rem;
}

.security-tips i {
  color: #10B981;
}

/* Responsive Design */
@media (max-width: 768px) {
  .reset-card {
    flex-direction: column;
    min-height: auto;
  }
  
  .reset-preview,
  .reset-form {
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
  
  .reset-form {
    max-width: none;
  }
}

@media (max-width: 480px) {
  .reset-container {
    padding: 1rem;
  }
  
  .reset-card {
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