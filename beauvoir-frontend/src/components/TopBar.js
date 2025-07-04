import React from 'react';

const TopBar = () => {
  return (
    <div style={{
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      padding: '1rem',
      borderBottom: '1px solid #eee',
      backgroundColor: '#f8f9fa'
    }}>
      <h1 style={{ margin: 0 }}>My account</h1>
      <div style={{ display: 'flex', gap: '1rem' }}>
        <div style={{ position: 'relative' }}>
          <input 
            type="text" 
            placeholder="Search for..." 
            style={{ 
              padding: '0.5rem 1rem',
              paddingLeft: '2.5rem',
              border: '1px solid #ddd',
              borderRadius: '4px'
            }}
          />
          <span style={{ position: 'absolute', left: '10px', top: '10px' }}>🔍</span>
        </div>
        <select style={{ 
          padding: '0.5rem 1rem',
          border: '1px solid #ddd',
          borderRadius: '4px',
          backgroundColor: 'white'
        }}>
        </select>
      </div>
    </div>
  );
};

export default TopBar;