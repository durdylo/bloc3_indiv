import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, tap } from 'rxjs';

export interface Camera {
  id: number;
  nom: string;
  code: string;
  estAfficher: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CameraService {
  private apiUrl = 'http://localhost:5000/api/cameras'; // Ajustez le port selon votre config
  
  // Sujet pour signaler les mises à jour de caméras
  private cameraUpdatedSource = new Subject<Camera>();
  cameraUpdated$ = this.cameraUpdatedSource.asObservable();

  constructor(private http: HttpClient) { }

  getCameras(): Observable<Camera[]> {
    return this.http.get<Camera[]>(this.apiUrl);
  }

  getCamera(id: number): Observable<Camera> {
    return this.http.get<Camera>(`${this.apiUrl}/${id}`);
  }

  addCamera(camera: Camera): Observable<Camera> {
    return this.http.post<Camera>(this.apiUrl, camera)
      .pipe(
        tap(newCamera => this.cameraUpdatedSource.next(newCamera))
      );
  }

  updateCamera(camera: Camera): Observable<any> {
    return this.http.put(`${this.apiUrl}/${camera.id}`, camera)
      .pipe(
        tap(() => this.cameraUpdatedSource.next(camera))
      );
  }

  deleteCamera(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}