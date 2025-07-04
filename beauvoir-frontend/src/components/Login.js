import React, { useState } from 'react';
import axios from 'axios';
import TopBar from './TopBar'; // Add this

export default function Login({ onLoginSuccess }) {
  // Aquí declaramos los "estados" que usaremos para guardar datos y mensajes
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState('');

  // Aquí definimos la función handleSubmit que se ejecutará cuando envíes el formulario
  const handleSubmit = async (e) => {
    e.preventDefault(); // Evita que el formulario recargue la página al enviar

    try {
      // Llamamos a la API para hacer login
      const response = await axios.post('http://localhost:5259/api/auth/login', {
        username,
        password
      });

      // Guardamos el token JWT en localStorage para usarlo después
      localStorage.setItem('token', response.data);

      setMessage('Login successful!');
      onLoginSuccess(); 
    } catch (error) {
      // Si ocurre un error, lo mostramos en pantalla
      if (error.response) {
        setMessage('Login failed: ' + error.response.data);
      } else if (error.request) {
        setMessage('No response from server');
      } else {
        setMessage('Error: ' + error.message);
      }
    }
  };

  return (
    <div>
      <TopBar />
      <div style={{
        maxWidth: '400px',
        margin: '2rem auto',
        padding: '2rem',
        border: '1px solid #e0e0e0',
        borderRadius: '8px'
      }}>
        <h2 style={{ textAlign: 'center', marginTop: 0 }}>Log In</h2>
        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.5rem' }}>Email Address</label>
            <input
              value={username}
              onChange={e => setUsername(e.target.value)}
              placeholder="Placeholder"
              required
              style={{
                width: '100%',
                padding: '0.75rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            />
          </div>
          <div style={{ marginBottom: '1.5rem' }}>
            <label style={{ display: 'block', marginBottom: '0.5rem' }}>Password</label>
            <input
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="Placeholder"
              required
              style={{
                width: '100%',
                padding: '0.75rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            />
          </div>
          <button 
            type="submit"
            style={{
              width: '100%',
              padding: '0.75rem',
              background: '#007bff',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer'
            }}
          >
            Log in
          </button>
        </form>
        
        <div style={{ 
          display: 'flex', 
          justifyContent: 'space-between',
          marginTop: '1.5rem',
          fontSize: '0.9rem'
        }}>
          
          <a href="#" style={{ color: '#007bff', textDecoration: 'none' }}>
            No account yet? Sign Up
          </a>
        </div>
        
        {message && <p style={{ 
          color: message.includes('successful') ? 'green' : 'red', 
          textAlign: 'center',
          marginTop: '1rem'
        }}>{message}</p>}
      </div>
    </div>
  );
}