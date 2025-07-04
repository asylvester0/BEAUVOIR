import React, { useState } from 'react';
import axios from 'axios';
import TopBar from './TopBar';
export default function Register() {
  const [form, setForm] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: ''
  });
  const [message, setMessage] = useState('');
  const handleChange = (e) => {
    setForm({...form, [e.target.name]: e.target.value});
  };
  const validatePassword = (password) => {
    const re = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;
    return re.test(password);
  };
  const handleSubmit = async (e) => {
    e.preventDefault();
    if (form.password !== form.confirmPassword) {
      setMessage("Passwords don't match");
      return;
    }
    if (!validatePassword(form.password)) {
      setMessage("Password must be at least 8 chars with uppercase, lowercase and number");
      return;
    }
    try {
      const response = await axios.post('http://localhost:5259/api/auth/register', {
        username: form.username,
        email: form.email,
        password: form.password,
        firstName: form.firstName,
        lastName: form.lastName
      });
      setMessage('Registration successful! You can now log in.');
    } catch (error) {
      setMessage('Registration failed: ' + (error.response?.data || error.message));
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
        <h2 style={{ textAlign: 'center', marginTop: 0 }}>Register</h2>
        <form onSubmit={handleSubmit}>
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.5rem' }}>Username</label>
            <input
              name="username"
              placeholder="Placeholder"
              value={form.username}
              onChange={handleChange}
              required
              style={{
                width: '100%',
                padding: '0.75rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            />
          </div>
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.5rem' }}>Email Address</label>
            <input
              type="email"
              name="email"
              placeholder="Placeholder"
              value={form.email}
              onChange={handleChange}
              required
              style={{
                width: '100%',
                padding: '0.75rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            />
          </div>
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.5rem' }}>Password</label>
            <input
              type="password"
              name="password"
              placeholder="Placeholder"
              value={form.password}
              onChange={handleChange}
              required
              style={{
                width: '100%',
                padding: '0.75rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            />
          </div>
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.5rem' }}>Confirm Password</label>
            <input
              type="password"
              name="confirmPassword"
              placeholder="Placeholder"
              value={form.confirmPassword}
              onChange={handleChange}
              required
              style={{
                width: '100%',
                padding: '0.75rem',
                border: '1px solid #ddd',
                borderRadius: '4px'
              }}
            />
          </div>
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ display: 'block', marginBottom: '0.5rem' }}>First Name</label>
            <input
              name="firstName"
              placeholder="Placeholder"
              value={form.firstName}
              onChange={handleChange}
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
            <label style={{ display: 'block', marginBottom: '0.5rem' }}>Last Name</label>
            <input
              name="lastName"
              placeholder="Placeholder"
              value={form.lastName}
              onChange={handleChange}
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
            Register
          </button>
        </form>
        
        <div style={{ 
          textAlign: 'center',
          marginTop: '1.5rem',
          fontSize: '0.9rem'
        }}>
          <a href="#" style={{ color: '#007bff', textDecoration: 'none' }}>
            Already have an account? Log in
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