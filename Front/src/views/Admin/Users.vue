<template>
  <div class="admin-container">
   

    <!-- Main Content -->
    <div class="main-card">
      <div class="card-header">
        <div class="header-content">
          <div class="title-section">
            <h2><i class="bi bi-people me-2"></i>Gestion des Utilisateurs</h2>
            <!-- <p>Gérez tous les utilisateurs de votre système</p> -->
          </div>
          
          <div class="actions-section">
            <div class="search-box">
              <input 
                type="text" 
                placeholder="Rechercher..." 
                v-model="searchQuery"
              />
              <button class="search-btn">
                <i class="bi bi-search"></i>
              </button>
            </div>
            
            <button class="btn-add" @click="showAddModal = true">
              <i class="bi bi-person-plus me-2"></i>Nouvel utilisateur
            </button>
            
            <button class="btn-refresh" @click="refreshUsers">
              <i class="bi bi-arrow-clockwise"></i>
            </button>
          </div>
        </div>
      </div>

      <div class="card-body">
        <!-- Loading State -->
        <div v-if="loading" class="loading-state">
          <div class="spinner"></div>
          <p>Chargement des utilisateurs...</p>
        </div>

        <!-- Empty State -->
        <div v-else-if="filteredUsers.length === 0" class="empty-state">
          <i class="bi bi-people"></i>
          <h3>Aucun utilisateur trouvé</h3>
          <button class="btn-retry" @click="refreshUsers">
            <i class="bi bi-arrow-clockwise me-2"></i>Réessayer
          </button>
        </div>

        <!-- Users Table -->
        <div v-else class="table-container">
          <table class="users-table">
            <thead>
              <tr>
               <!-- <th>ID</th>-->
                <th>Utilisateur</th>
                <th>Email</th>
                <th>Téléphone</th>
                <th>Rôle</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="user in filteredUsers" :key="user.id">
               <!-- <td>{{ user.id }}</td>-->
                <td>
                  <div class="user-info">
                    <div class="user-avatar">
                      {{ user.username.charAt(0).toUpperCase() }}
                    </div>
                    <div class="user-details">
                      <strong>{{ user.username }}</strong>
                    </div>
                  </div>
                </td>
                <td>{{ user.email }}</td>
                <td>{{ user.phoneNumber || 'N/A' }}</td>
                <td>
                  <span :class="['role-badge', user.roles.includes('Admin') ? 'admin' : 'user']">
                    {{ user.roles.join(', ') }}
                  </span>
                </td>
                <td>
                  <div class="action-buttons">
                    <button class="btn-edit" @click="editUser(user)" title="Modifier">
                      <i class="bi bi-pencil"></i>
                    </button>
                  
<button 
  class="btn-role" 
  @click="toggleRole(user)"
  :title="user.roles.includes('Admin') ? 'Rétrograder' : 'Promouvoir'"
  :class="{'disabled-role-btn': user.roles.includes('Admin') && adminCount <= 1}"
  :disabled="user.roles.includes('Admin') && adminCount <= 1"
>
  <i :class="user.roles.includes('Admin') ? 'bi bi-person-down' : 'bi bi-person-up'"></i>
</button>
<button 
  class="btn-delete" 
  @click="!user.roles.includes('Admin') && deleteUser(user)"  
  :title="user.roles.includes('Admin') ? 'Impossible de supprimer un admin' : 'Supprimer'"
  :class="{'admin-delete-btn': user.roles.includes('Admin')}"
>
  <i class="bi bi-trash"></i>
</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Add/Edit User Modal -->
    <div v-if="showAddModal || showEditModal" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>
            <i class="bi me-2" :class="showEditModal ? 'bi-person-gear' : 'bi-person-plus'"></i>
            {{ showEditModal ? 'Modifier Utilisateur' : 'Nouvel Utilisateur' }}
          </h2>
          <button class="close-btn" @click="closeModal">
            <i class="bi bi-x-lg"></i>
          </button>
        </div>

        <div class="modal-body">
          <!-- Error Messages -->
          <div v-if="serverErrors.length" class="error-alert">
            <div v-for="(error, index) in serverErrors" :key="index" class="error-item">
              <i class="bi bi-exclamation-circle"></i>
              <span>{{ error }}</span>
            </div>
          </div>

          <!-- Form Fields -->
          <div class="form-group">
            <label>Nom d'utilisateur <span class="required">*</span></label>
            <input
              v-model="formData.username"
              type="text"
              placeholder="Entrez le nom d'utilisateur"
              :class="{ 'error': formSubmitted && !formData.username }"
            />
            <span v-if="formSubmitted && !formData.username" class="error-message">
              <i class="bi bi-exclamation-circle"></i> Ce champ est requis
            </span>
          </div>

          <div class="form-group">
            <label>Email <span class="required">*</span></label>
            <input
              v-model="formData.email"
              type="email"
              placeholder="Entrez l'email"
              :disabled="showEditModal"
              :class="{ 
                'error': formSubmitted && !formData.email,
                'disabled': showEditModal
              }"
            />
            <span v-if="formSubmitted && !formData.email" class="error-message">
              <i class="bi bi-exclamation-circle"></i> Ce champ est requis
            </span>
          </div>

          <div class="form-group">
            <label>Numéro de téléphone</label>
            <input
              v-model="formData.phoneNumber"
              type="tel"
              placeholder="Entrez le numéro de téléphone"
            />
          </div>

          <div v-if="showAddModal" class="form-group">
            <label>Mot de passe <span class="required">*</span></label>
            <input
              v-model="formData.password"
              type="password"
              placeholder="Entrez le mot de passe"
              :class="{ 'error': formSubmitted && !formData.password }"
            />
            <span v-if="formSubmitted && !formData.password" class="error-message">
              <i class="bi bi-exclamation-circle"></i> Ce champ est requis
            </span>
          </div>

          <div class="form-group">
            <label>Rôles <span class="required">*</span></label>
            <select
              v-model="formData.roles"
              multiple
              :class="{ 'error': formSubmitted && formData.roles.length === 0 }"
            >
              <option value="User">Utilisateur</option>
              <option value="Admin">Administrateur</option>
            </select>
            <span v-if="formSubmitted && formData.roles.length === 0" class="error-message">
              <i class="bi bi-exclamation-circle"></i> Sélectionnez au moins un rôle
            </span>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn-cancel" @click="closeModal">
            Annuler
          </button>
          <button 
            class="btn-submit" 
            @click="submitForm"
            :disabled="processing"
          >
            <span v-if="processing" class="spinner-border spinner-border-sm"></span>
            {{ showEditModal ? 'Modifier' : 'Créer' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useStore } from 'vuex';
import Cookies from 'js-cookie';

export default {
  name: "UserManagement",
  setup() {
    const router = useRouter();
    const store = useStore();
    
    // Data
    const users = ref([]);
    const loading = ref(true);
    const processing = ref(false);
    const searchQuery = ref('');
    const showAddModal = ref(false);
    const showEditModal = ref(false);
    const formData = ref({
      username: '',
      email: '',
      phoneNumber: '',
      password: '',
      roles: ['User']
    });
    const editingUserId = ref(null);
    const formSubmitted = ref(false);
    const serverErrors = ref([]);
    const currentUserId = ref(null);

    // Computed
    const adminCount = computed(() => users.value.filter(u => u.roles.includes('Admin')).length);
    
    const filteredUsers = computed(() => {
      const query = searchQuery.value.toLowerCase();
      let filtered = users.value;
      
      // Exclure l'utilisateur courant si currentUserId est défini
      if (currentUserId.value) {
        filtered = filtered.filter(user => user.id !== currentUserId.value);
      }
      
      if (!searchQuery.value) return filtered;
      
      return filtered.filter(user => 
        user.username.toLowerCase().includes(query) || 
        user.email.toLowerCase().includes(query) ||
        (user.phoneNumber && user.phoneNumber.includes(query))
      );
    });

    // Methods
    const checkAdminAccess = () => {
      const token = Cookies.get('token');
      if (!token) {
        store.dispatch('logout');
        router.push('/login');
        return false;
      }

      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        if (payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] !== 'Admin') {
          alert("Accès réservé aux administrateurs");
          router.push('/dashboard');
          return false;
        }
        currentUserId.value = payload.sub; // Stocker l'ID de l'utilisateur courant
        return true;
      } catch {
        return false;
      }
    };

    const fetchUsers = async () => {
      if (!checkAdminAccess()) return;
      
      loading.value = true;
      try {
        const token = Cookies.get('token');
        const response = await axios.get('/api/users/all', {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        
        users.value = Array.isArray(response.data) ? response.data : [];
      } catch (error) {
        handleError(error);
      } finally {
        loading.value = false;
      }
    };

    const handleError = (error) => {
      let message = "Erreur lors de l'opération";
      if (error.response) {
        if (error.response.status === 401) {
          message = "Session expirée - Veuillez vous reconnecter";
          store.dispatch('logout');
          router.push('/login');
        } else if (error.response.data?.message) {
          message = error.response.data.message;
        }
      }
      alert(message);
    };

    const refreshUsers = () => {
      searchQuery.value = '';
      fetchUsers();
    };

    const deleteUser = async (user) => {
      try {
        const userId = user.id;
        console.log("Attempting to delete user ID:", userId);
        
        if (!userId) {
          throw new Error("ID utilisateur manquant");
        }

        // Empêcher la suppression du dernier admin
        if (user.roles.includes('Admin') && adminCount.value <= 1) {
          alert("Vous ne pouvez pas supprimer le dernier administrateur");
          return;
        }

        if (!confirm(`Confirmez la suppression de ${user.username} (ID: ${userId}) ?`)) {
          return;
        }

        const token = Cookies.get('token');
        const response = await axios.delete(`/api/users/delete/${userId}`, {
          headers: { 'Authorization': `Bearer ${token}` }
        });

        console.log("Delete response:", response.data);

        if (response.data?.success) {
          // Suppression optimiste
          users.value = users.value.filter(u => u.id !== userId);
          alert("Utilisateur supprimé avec succès");
          
          // Rafraîchir après 1 seconde
          setTimeout(fetchUsers, 1000);
        } else {
          throw new Error(response.data?.message || "Échec de la suppression");
        }
      } catch (error) {
        console.error("Delete error:", error);
        alert(`Erreur: ${error.message}\nL'utilisateur sera rechargé.`);
        fetchUsers();
      }
    };

    const toggleRole = async (user) => {
      const newRole = user.roles.includes('Admin') ? ['User'] : ['Admin'];
      const action = newRole[0] === 'Admin' ? 'promouvoir' : 'rétrograder';
      
      // Empêcher la rétrogradation du dernier admin
      if (user.roles.includes('Admin') && adminCount.value <= 1) {
        alert("Vous ne pouvez pas rétrograder le dernier administrateur");
        return;
      }
      
      if (!confirm(`Confirmez-vous vouloir ${action} cet utilisateur ?`)) return;
      
      try {
        const token = Cookies.get('token');
        await axios.put(`/api/users/edit/${user.id}`, {
          username: user.username,
          email: user.email,
          phoneNumber: user.phoneNumber,
          roles: newRole
        }, {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        
        // Update local data
        const index = users.value.findIndex(u => u.id === user.id);
        if (index !== -1) {
          users.value[index].roles = newRole;
        }
      } catch (error) {
        handleError(error);
      }
    };

    const editUser = (user) => {
      editingUserId.value = user.id;
      formData.value = {
        username: user.username,
        email: user.email,
        phoneNumber: user.phoneNumber || '',
        password: '',
        roles: [...user.roles]
      };
      showEditModal.value = true;
      formSubmitted.value = false;
      serverErrors.value = [];
    };

    const closeModal = () => {
      showAddModal.value = false;
      showEditModal.value = false;
      editingUserId.value = null;
      formData.value = {
        username: '',
        email: '',
        phoneNumber: '',
        password: '',
        roles: ['User']
      };
      formSubmitted.value = false;
      serverErrors.value = [];
    };

    const submitForm = async () => {
      formSubmitted.value = true;
      serverErrors.value = [];
      processing.value = true;

      // Validate required fields
      if (!formData.value.username || !formData.value.email || 
          (showAddModal.value && !formData.value.password) || 
          formData.value.roles.length === 0) {
        processing.value = false;
        return;
      }

      try {
        const token = Cookies.get('token');
        const config = {
          headers: { 'Authorization': `Bearer ${token}` }
        };

        if (showEditModal.value) {
          await axios.put(`/api/users/edit/${editingUserId.value}`, {
            username: formData.value.username,
            email: formData.value.email,
            phoneNumber: formData.value.phoneNumber,
            roles: formData.value.roles
          }, config);
          alert("Utilisateur modifié avec succès");
        } else {
          await axios.post('/api/users/add', formData.value, config);
          alert("Utilisateur créé avec succès");
        }

        closeModal();
        refreshUsers();
      } catch (error) {
        if (error.response?.data?.errors) {
          serverErrors.value = Object.values(error.response.data.errors).flat();
        } else if (error.response?.data?.message) {
          serverErrors.value = [error.response.data.message];
        } else {
          serverErrors.value = ["Une erreur inconnue est survenue"];
        }
      } finally {
        processing.value = false;
      }
    };

    onMounted(fetchUsers);

    return {
      users,
      loading,
      processing,
      searchQuery,
      showAddModal,
      showEditModal,
      formData,
      adminCount,
      filteredUsers,
      formSubmitted,
      serverErrors,
      refreshUsers,
      deleteUser,
      toggleRole,
      editUser,
      closeModal,
      submitForm
    };
  }
};
</script>

<style scoped>
.admin-container {
  padding: 2rem;
  max-width: 100%;
}

/* Stats Row */
.stats-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  border-radius: 12px;
  padding: 1.5rem;
  color: white;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
}

.stat-card.primary {
  background: linear-gradient(135deg, #4e73df 0%, #224abe 100%);
}

.stat-card.info {
  background: linear-gradient(135deg, #36b9cc 0%, #1a8a9c 100%);
}

.stat-card.success {
  background: linear-gradient(135deg, #1cc88a 0%, #13855c 100%);
}

.stat-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.stat-text h3 {
  margin: 0;
  font-size: 1rem;
  font-weight: 500;
  opacity: 0.9;
}

.stat-text h1 {
  margin: 0.5rem 0;
  font-size: 2rem;
  font-weight: 700;
}

.stat-text p {
  margin: 0;
  font-size: 0.85rem;
  opacity: 0.8;
}

.stat-icon {
  font-size: 2.5rem;
  opacity: 0.2;
}

/* Main Card */
.main-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}

.card-header {
  padding: 1.5rem;
  border-bottom: 1px solid #eee;
}

.header-content {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}

.title-section h2 {
  margin: 0;
  font-size: 1.5rem;
  color: #2c3e50;
  display: flex;
  align-items: center;
}

.title-section p {
  margin: 0.25rem 0 0;
  color: #6c757d;
  font-size: 0.9rem;
}

.actions-section {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.search-box {
  display: flex;
  border: 1px solid #ddd;
  border-radius: 8px;
  overflow: hidden;
  background: white;
}

.search-box input {
  flex: 1;
  padding: 0.5rem 1rem;
  border: none;
  outline: none;
  min-width: 250px;
}

.search-btn {
  padding: 0 1rem;
  border: none;
  background: #f8f9fa;
  color: #6c757d;
  cursor: pointer;
  border-left: 1px solid #ddd;
}

.search-btn:hover {
  background: #e9ecef;
}

.btn-add {
  padding: 0.5rem 1rem;
  background: #3498db;
  color: white;
  border: none;
  border-radius: 4px;
  display: flex;
  align-items: center;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
  font-size: 1rem;
}

.btn-add:hover {
  background: #2980b9;
}

.btn-refresh {
  padding: 0.5rem;
  background: #f8f9fa;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-refresh:hover {
  background: #e9ecef;
}

/* Table */
.table-container {
  overflow-x: auto;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
}

.users-table th,
.users-table td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid #eee;
}

.users-table th {
  background: #f8f9fa;
  font-weight: 600;
  color: #495057;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: #4e73df;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
}

.role-badge {
  padding: 0.35rem 0.75rem;
  border-radius: 50px;
  font-size: 0.85rem;
  font-weight: 500;
}

.role-badge.user {
  background: #e3f2fd;
  color: #1976d2;
}

.role-badge.admin {
  background: #ffebee;
  color: #d32f2f;
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.action-buttons button {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-edit {
  background: #e3f2fd;
  color: #1976d2;
}

.btn-edit:hover {
  background: #bbdefb;
}

.btn-role {
  background: #fff8e1;
  color: #ffa000;
}
.disabled-role-btn {
  background: #f5f5f5 !important;
  color: #bdbdbd !important;
  cursor: not-allowed !important;
}

.btn-role:hover {
  background: #ffecb3;
}

.btn-delete {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.2s;
  background: #ffebee;
  color: #d32f2f;
}


.btn-delete:hover {
  background: #ffcdd2;
}
.admin-delete-btn {
  background: #f5f5f5 !important;
  color: #bdbdbd !important;
  cursor: not-allowed !important;
  pointer-events: none;
}

.admin-delete-btn i::before {
  content: "\F47A"; /* Icône de verrou de Bootstrap Icons */
}

/* Loading State */
.loading-state {
  padding: 3rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #6c757d;
}

.loading-state .spinner {
  width: 3rem;
  height: 3rem;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* Empty State */
.empty-state {
  padding: 3rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #6c757d;
}

.empty-state i {
  font-size: 3rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.empty-state h3 {
  margin: 0 0 1rem;
  font-weight: 500;
}

.btn-retry {
  padding: 0.5rem 1.25rem;
  background: #f8f9fa;
  border: 1px solid #ddd;
  border-radius: 8px;
  display: flex;
  align-items: center;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-retry:hover {
  background: #e9ecef;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 1rem;
}

.modal-content {
  background: white;
  border-radius: 12px;
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
}

.modal-header {
  padding: 1.5rem;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  color: #6c757d;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
}

.close-btn:hover {
  background: #f8f9fa;
}

.modal-body {
  padding: 1.5rem;
}

.error-alert {
  background: #ffebee;
  border-left: 4px solid #d32f2f;
  padding: 1rem;
  border-radius: 4px;
  margin-bottom: 1.5rem;
}

.error-item {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  color: #d32f2f;
}

.error-item:not(:last-child) {
  margin-bottom: 0.5rem;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #495057;
}

.required {
  color: #d32f2f;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
  transition: border 0.2s;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: #4e73df;
  box-shadow: 0 0 0 3px rgba(78, 115, 223, 0.1);
}

.form-group input.error,
.form-group select.error {
  border-color: #d32f2f;
}

.form-group input.disabled {
  background: #f8f9fa;
  cursor: not-allowed;
}

.form-group select {
  height: auto;
  min-height: 120px;
}

.error-message {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: #d32f2f;
  font-size: 0.85rem;
  margin-top: 0.25rem;
}

.modal-footer {
  padding: 1.5rem;
  border-top: 1px solid #eee;
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

.btn-cancel {
  padding: 0.75rem 1.5rem;
  background: #ddd;
  border: 1px solid #ddd;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
}

.btn-cancel:hover {
  background: #e9ecef;
}

.btn-submit {
  padding: 0.75rem 1.5rem;
  background: #4e73df;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.btn-submit:hover {
  background: #3e63cf;
}

.btn-submit:disabled {
  background: #95a5a6;
  cursor: not-allowed;
}

/* Responsive */
@media (max-width: 768px) {
  .admin-container {
    padding: 1rem;
  }
  
  .header-content {
    flex-direction: column;
    align-items: stretch;
  }
  
  .actions-section {
    flex-wrap: wrap;
  }
  
  .search-box {
    min-width: 100%;
  }
  
  .users-table th,
  .users-table td {
    padding: 0.75rem;
  }
  
  .modal-content {
    max-height: 80vh;
  }
}

@media (max-width: 576px) {
  .stats-row {
    grid-template-columns: 1fr;
  }
  
  .action-buttons {
    flex-direction: column;
  }
  
  .action-buttons button {
    width: 100%;
  }
}
</style>
