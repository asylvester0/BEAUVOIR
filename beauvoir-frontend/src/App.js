import React, { useState, useEffect } from 'react';
import Login from './components/Login';
import Register from './components/Register';
import { fetchUserProfile } from './api/user';
import ModelDashboard from './components/ModelDashboard';
import { FriendsList, FriendRequests, SendFriendRequest } from './components/Friend'; // ajusta ruta
import TopBar from './components/TopBar';

function App() {
  const [view, setView] = useState('login'); // 'login' or 'register'
  const [isLoggedIn, setIsLoggedIn] = useState(!!localStorage.getItem('token'));
  const [user, setUser] = useState(null);
  const [dashboardView, setDashboardView] = useState('models'); // 'models', 'friends', 'requests', 'sendRequest'
  const token = localStorage.getItem('token');

  const handleLoginSuccess = () => {
    setIsLoggedIn(true);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    setIsLoggedIn(false);
    setView('login');
  };

  useEffect(() => {
    if (isLoggedIn && token) {
      fetchUserProfile(token)
        .then(userData => setUser(userData))
        .catch(() => {
          // token inválido o expirado, forzamos logout
          handleLogout();
        });
    }
  }, [isLoggedIn, token]);

  if (isLoggedIn && user) {
    return (
      <div>
        <TopBar />
        {/* Remove the welcome message and email display */}
        <div style={{ display: 'flex', marginTop: '1rem' }}>
          {/* Navigation buttons - style them */}
          <nav style={{ 
            display: 'flex', 
            flexDirection: 'column',
            width: '200px',
            padding: '1rem',
            borderRight: '1px solid #eee'
          }}>
            <button 
              onClick={() => setDashboardView('models')} 
              style={{
                padding: '0.75rem 1rem',
                marginBottom: '0.5rem',
                textAlign: 'left',
                background: dashboardView === 'models' ? '#e9ecef' : 'transparent',
                border: 'none',
                cursor: 'pointer'
              }}
            >
              Modelos
            </button>
            <button 
              onClick={() => setDashboardView('friends')} 
              style={{
                padding: '0.75rem 1rem',
                marginBottom: '0.5rem',
                textAlign: 'left',
                background: dashboardView === 'friends' ? '#e9ecef' : 'transparent',
                border: 'none',
                cursor: 'pointer'
              }}
            >
              Amigos
            </button>
            <button 
              onClick={() => setDashboardView('requests')} 
              style={{
                padding: '0.75rem 1rem',
                marginBottom: '0.5rem',
                textAlign: 'left',
                background: dashboardView === 'requests' ? '#e9ecef' : 'transparent',
                border: 'none',
                cursor: 'pointer'
              }}
            >
              Solicitudes
            </button>
            <button 
              onClick={() => setDashboardView('sendRequest')} 
              style={{
                padding: '0.75rem 1rem',
                textAlign: 'left',
                background: dashboardView === 'sendRequest' ? '#e9ecef' : 'transparent',
                border: 'none',
                cursor: 'pointer'
              }}
            >
              Enviar Solicitud
            </button>
          </nav>

          {/* Content area */}
          <div style={{ flex: 1, padding: '1rem 2rem' }}>
   

        {/* Contenido según vista seleccionada */}
       {dashboardView === 'models' && <ModelDashboard token={token} />}
        {dashboardView === 'friends' && <FriendsList token={token} />}
        {dashboardView === 'requests' && <FriendRequests token={token} />}
        {dashboardView === 'sendRequest' && <SendFriendRequest token={token} />}
               {/* ... existing dashboard views ... */}
          </div>
        </div>
      </div>
    );
  }
    
  

  return (
    <div>
      <nav>
        <button onClick={() => setView('login')} disabled={view === 'login'}>Login</button>
        <button onClick={() => setView('register')} disabled={view === 'register'}>Register</button>
      </nav>

      {view === 'login' ? <Login onLoginSuccess={handleLoginSuccess} /> : <Register />}
    </div>
  );
}

export default App;
