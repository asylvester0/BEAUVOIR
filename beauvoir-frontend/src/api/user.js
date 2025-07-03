import axios from 'axios';

const API_URL = 'http://localhost:5259/api';

export async function fetchUserProfile(token) {
  const response = await axios.get(`${API_URL}/auth/me`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return response.data;
}
