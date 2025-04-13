import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, tap } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface MurImage {
  id: number;
  nom: string;
  positions: Position[];
}

export interface Position {
  id: number;
  idMurImage: number;
  codeCamera: string | null;
  estActif: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class MurImageService {
  private murService =   environment.murImageServiceUrl
  private apiUrlMurImages = this.murService + '/api/murimages/'; // Ajustez le port selon votre config
  private apiUrlPositions = this.murService + '/api/positions/'; // Ajustez le port selon votre config
  // Sujet pour signaler les mises à jour de positions
  private positionUpdatedSource = new Subject<Position>();
  positionUpdated$ = this.positionUpdatedSource.asObservable();

  constructor(private http: HttpClient) { }

  // MurImage methods
  getMurImages(): Observable<MurImage[]> {
    console.log("url", this.apiUrlMurImages);
    return this.http.get<MurImage[]>(this.apiUrlMurImages);
  }

  getMurImage(id: number): Observable<MurImage> {
    return this.http.get<MurImage>(`${this.apiUrlMurImages}/${id}`);
  }

  addMurImage(murImage: MurImage): Observable<MurImage> {
    return this.http.post<MurImage>(this.apiUrlMurImages, murImage);
  }

  updateMurImage(murImage: MurImage): Observable<any> {
    return this.http.put(`${this.apiUrlMurImages}/${murImage.id}`, murImage);
  }

  deleteMurImage(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrlMurImages}/${id}`);
  }

  // Position methods
  getPositions(): Observable<Position[]> {
    return this.http.get<Position[]>(this.apiUrlPositions);
  }

  getPositionsByMurImage(murImageId: number): Observable<Position[]> {
    return this.http.get<Position[]>(`${this.apiUrlPositions}/MurImage/${murImageId}`);
  }

  addPosition(position: Position): Observable<Position> {
    return this.http.post<Position>(this.apiUrlPositions, position)
      .pipe(
        tap(newPosition => this.positionUpdatedSource.next(newPosition))
      );
  }

  updatePosition(position: Position): Observable<any> {
    return this.http.put(`${this.apiUrlPositions}/${position.id}`, position)
      .pipe(
        tap(() => this.positionUpdatedSource.next(position))
      );
  }

  deletePosition(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrlPositions}/${id}`);
  }
  
  // Méthode pour mettre à jour uniquement le codeCamera d'une position
  updatePositionCamera(positionId: number, codeCamera: string | null): Observable<any> {
    return this.http.get<Position>(`${this.apiUrlPositions}/${positionId}`)
      .pipe(
        tap(position => {
          position.codeCamera = codeCamera;
          return this.updatePosition(position);
        })
      );
  }
}