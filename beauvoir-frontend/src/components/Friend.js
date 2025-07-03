import React, { useEffect, useState } from "react";
import axios from "axios";

const apiBase = "http://localhost:5259/api/friendship";

function FriendsList({ token }) {
  const [friends, setFriends] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchFriends = () => {
    setLoading(true);
    setError(null);
    axios
      .get(`${apiBase}/list`, { headers: { Authorization: `Bearer ${token}` } })
      .then((res) => setFriends(res.data))
      .catch(() => setError("Error cargando amigos"))
      .finally(() => setLoading(false));
  };

  // Eliminar amigo no implementado aún, solo aviso
  const removeFriend = (friendId) => {
    alert(
      "No hay endpoint para eliminar amigos en el backend actual. ¿Quieres que te ayude a agregarlo?"
    );
  };

  useEffect(() => {
    if(token) fetchFriends();
  }, [token]);

  if (loading) return <p>Cargando amigos...</p>;
  if (error) return <p style={{color: "red"}}>{error}</p>;
  if (friends.length === 0) return <p>No tienes amigos aún.</p>;

 return (
    <div>
      <h2>Mis Amigos</h2>
      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', 
        gap: '1.5rem',
        marginTop: '1.5rem'
      }}>
        {friends.map((f) => (
          <div key={f.id} style={{ 
            border: '1px solid #e0e0e0', 
            borderRadius: '8px', 
            padding: '1rem',
            display: 'flex',
            flexDirection: 'column'
          }}>
            <h3 style={{ marginTop: 0 }}>{f.firstName} {f.lastName}</h3>
            <p style={{ color: '#666' }}>@{f.username}</p>
            <button 
              onClick={() => removeFriend(f.id)}
              style={{
                marginTop: 'auto',
                padding: '0.5rem',
                background: '#dc3545',
                color: 'white',
                border: 'none',
                borderRadius: '4px',
                cursor: 'pointer',
                alignSelf: 'flex-start'
              }}
            >
              Eliminar
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}

function FriendRequests({ token }) {
  const [requests, setRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchRequests = () => {
    setLoading(true);
    setError(null);
    axios
      .get(`${apiBase}/requests`, { headers: { Authorization: `Bearer ${token}` } })
      .then((res) => setRequests(res.data))
      .catch(() => setError("Error cargando solicitudes"))
      .finally(() => setLoading(false));
  };

  const respondRequest = (userId, accept) => {
    const url = accept
      ? `${apiBase}/accept/${userId}`
      : `${apiBase}/reject/${userId}`;
    axios
      .post(url, {}, { headers: { Authorization: `Bearer ${token}` } })
      .then(() => fetchRequests())
      .catch(() => alert("Error respondiendo solicitud"));
  };

  useEffect(() => {
    if(token) fetchRequests();
  }, [token]);

  if (loading) return <p>Cargando solicitudes...</p>;
  if (error) return <p style={{color: "red"}}>{error}</p>;
  if (requests.length === 0) return <p>No tienes solicitudes pendientes.</p>;

  return (
    <div>
      <h2>Solicitudes de Amistad</h2>
      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))', 
        gap: '1.5rem',
        marginTop: '1.5rem'
      }}>
        {requests.map((r) => (
          <div key={r.id} style={{ 
            border: '1px solid #e0e0e0', 
            borderRadius: '8px', 
            padding: '1rem',
            display: 'flex',
            flexDirection: 'column'
          }}>
            <h3 style={{ marginTop: 0 }}>{r.firstName} {r.lastName}</h3>
            <p style={{ color: '#666' }}>@{r.username}</p>
            <p style={{ fontSize: '0.9rem', color: '#666' }}>Enviado el: {new Date(r.createdAt).toLocaleDateString()}</p>
            <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
              <button 
                onClick={() => respondRequest(r.id, true)}
                style={{
                  flex: 1,
                  padding: '0.5rem',
                  background: '#28a745',
                  color: 'white',
                  border: 'none',
                  borderRadius: '4px',
                  cursor: 'pointer'
                }}
              >
                Aceptar
              </button>
              <button 
                onClick={() => respondRequest(r.id, false)}
                style={{
                  flex: 1,
                  padding: '0.5rem',
                  background: '#dc3545',
                  color: 'white',
                  border: 'none',
                  borderRadius: '4px',
                  cursor: 'pointer'
                }}
              >
                Rechazar
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function SendFriendRequest({ token }) {
  const [userId, setUserId] = useState("");
  const [message, setMessage] = useState("");

  const sendRequest = () => {
    if (!userId) {
      setMessage('Debes ingresar un ID de usuario válido');
      return;
    }
     axios
      .post(`${apiBase}/request/${userId}`, {}, { headers: { Authorization: `Bearer ${token}` } })
      .then(() => setMessage('Solicitud enviada'))
      .catch((err) => {
        setMessage(
          err.response?.data || 'Error enviando solicitud de amistad'
        );
      });
  };


  return (
    <div>
      <h2>Enviar Solicitud de Amistad</h2>
      <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem' }}>
        <input
          type="number"
          placeholder="ID usuario"
          value={userId}
          onChange={(e) => setUserId(e.target.value)}
          style={{
            padding: '0.75rem',
            border: '1px solid #ddd',
            borderRadius: '4px',
            flex: 1
          }}
        />
        <button 
          onClick={sendRequest}
          style={{
            padding: '0.75rem 1.5rem',
            background: '#007bff',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          Enviar solicitud
        </button>
      </div>
      {message && <p style={{ marginTop: '1rem' }}>{message}</p>}
    </div>
  );
}
export { FriendsList, FriendRequests, SendFriendRequest };
