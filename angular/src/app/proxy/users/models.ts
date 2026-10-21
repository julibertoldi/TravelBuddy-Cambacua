
export interface PublicUserProfileDto {
  userId?: string;
  nombre?: string;
  apellido?: string;
  fotoPerfilUrl?: string;
}

export interface UserProfileDto {
  userId?: string;
  userName?: string;
  nombre: string;
  apellido: string;
  email?: string;
  fotoPerfilUrl?: string;
  preferencias?: string;
}

export interface UpdateUserProfileDto {
  nombre: string;
  apellido: string;
  fotoPerfilUrl?: string | null;
  preferencias?: string | null;
}
