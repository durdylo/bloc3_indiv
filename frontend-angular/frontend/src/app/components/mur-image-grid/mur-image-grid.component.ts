import { Component, OnInit, OnDestroy } from '@angular/core';
import { MurImage, Position, MurImageService } from '../../services/mur-image.service';
import { Camera, CameraService } from '../../services/camera.service';
import { forkJoin, Subscription } from 'rxjs';

declare var bootstrap: any; // Pour utiliser Bootstrap JS

@Component({
  selector: 'app-mur-image-grid',
  templateUrl: './mur-image-grid.component.html',
  styleUrls: ['./mur-image-grid.component.scss']
})
export class MurImageGridComponent implements OnInit, OnDestroy {
  murImages: MurImage[] = [];
  cameras: Map<string, Camera> = new Map();
  availableCameras: Camera[] = [];
  loading = true;
  error = '';
  private cameraSubscription: Subscription | null = null;
  private refreshTimer: any;

  // Pour le modal
  selectedPosition: Position | null = null;
  selectedCamera: Camera | null = null;
  private modalInstance: any = null;

  constructor(
    private murImageService: MurImageService,
    private cameraService: CameraService
  ) { }

  ngOnInit(): void {
    this.loadData();
    
    // S'abonner aux mises à jour de caméras
    this.cameraSubscription = this.cameraService.cameraUpdated$.subscribe(camera => {
      // Mettre à jour la caméra dans notre Map local
      this.cameras.set(camera.code, camera);
      
      // Recharger les caméras disponibles si nécessaire
      this.updateAvailableCameras();
    });
    
    // Configurer un rafraîchissement périodique pour détecter les mises à jour
    this.refreshTimer = setInterval(() => {
      this.refreshData();
    }, 5000); // Toutes les 5 secondes
  }
  
  refreshData(): void {
    this.murImageService.getMurImages().subscribe({
      next: (data) => {
        this.murImages = data;
      },
      error: (err) => {
        console.error('Erreur lors du rafraîchissement des données', err);
      }
    });
  }
  
  ngOnDestroy(): void {
    // Se désabonner pour éviter les fuites de mémoire
    if (this.cameraSubscription) {
      this.cameraSubscription.unsubscribe();
    }
    
    // Arrêter le timer de rafraîchissement
    if (this.refreshTimer) {
      clearInterval(this.refreshTimer);
    }
  }

  loadData(): void {
    this.loading = true;
    
    // Charger à la fois les murs d'images et les caméras
    forkJoin({
      murImages: this.murImageService.getMurImages(),
      cameras: this.cameraService.getCameras()
    }).subscribe({
      next: (data) => {
        this.murImages = data.murImages;
        
        // Créer un Map pour un accès facile aux caméras par code
        data.cameras.forEach(camera => {
          this.cameras.set(camera.code, camera);
        });
        
        // Mettre à jour les caméras disponibles
        this.updateAvailableCameras();
        
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erreur lors du chargement des données';
        console.error(err);
        this.loading = false;
      }
    });
  }
  
  // Mettre à jour la liste des caméras disponibles
  updateAvailableCameras(): void {
    this.availableCameras = Array.from(this.cameras.values())
      .filter(camera => camera.estAfficher);
  }
  
  // Méthode pour vérifier si une caméra est affichable
  isCameraDisplayable(codeCamera: string | null | undefined): boolean {
    if (!codeCamera) return false;
    
    const camera = this.cameras.get(codeCamera);
    return camera ? camera.estAfficher : false;
  }

  // Obtenir le nom de la caméra à partir du code
  getCameraNameByCode(codeCamera: string | null | undefined): string {
    if (!codeCamera) return 'Aucune caméra';
    
    const camera = this.cameras.get(codeCamera);
    return camera ? camera.nom : 'Caméra inconnue';
  }

  // Méthode pour organiser les positions en grille 3x3
  getPositionGrid(positions: Position[]): (Position | null)[][] {
    const grid: (Position | null)[][] = [[], [], []];
    
    // Remplir la grille 3x3 avec les positions
    for (let i = 0; i < 3; i++) {
      for (let j = 0; j < 3; j++) {
        const index = i * 3 + j;
        grid[i][j] = positions[index] || null;
      }
    }
    
    return grid;
  }
  
  // Méthode pour calculer le nombre de positions actives
  getActivePositionsCount(positions: Position[]): number {
    return positions.filter(p => 
      p.codeCamera && 
      this.isCameraDisplayable(p.codeCamera) && 
      p.estActif
    ).length;
  }
  
  // Méthode pour supprimer une caméra d'une position
  removeCamera(position: Position | null): void {
    if (!position) return;
    
    // Cloner la position pour éviter des références inattendues
    const positionToUpdate = { ...position };
    positionToUpdate.codeCamera = null;
    
    this.murImageService.updatePosition(positionToUpdate).subscribe({
      next: () => {
        // Mettre à jour localement
        position.codeCamera = null;
      },
      error: (err) => {
        console.error('Erreur lors de la suppression de la caméra de la position', err);
        this.error = 'Erreur lors de la mise à jour de la position';
      }
    });
  }
  
  // Méthode pour afficher le modal d'ajout de caméra
  showAddCameraModal(position: Position): void {
    if (!position) return;
    
    this.selectedPosition = position;
    this.selectedCamera = null;
    
    // Mettre à jour la liste des caméras disponibles
    this.updateAvailableCameras();
    
    // Initialiser et afficher le modal Bootstrap
    const modalElement = document.getElementById('addCameraModal');
    if (modalElement) {
      this.modalInstance = new bootstrap.Modal(modalElement);
      this.modalInstance.show();
    }
  }
  
  // Méthode pour ajouter une caméra à une position
  confirmAddCamera(): void {
    if (!this.selectedPosition || !this.selectedCamera) return;
    
    // Mettre à jour la position avec la nouvelle caméra
    const updatedPosition: Position = {
      ...this.selectedPosition,
      codeCamera: this.selectedCamera.code,
      estActif: true // Initialisé à true lors de l'ajout
    };
    
    this.murImageService.updatePosition(updatedPosition).subscribe({
      next: () => {
        // Mettre à jour localement
        if (this.selectedPosition) {
          this.selectedPosition.codeCamera = this.selectedCamera!.code;
          this.selectedPosition.estActif = true;
        }
        
        // Réinitialiser les sélections
        this.selectedPosition = null;
        this.selectedCamera = null;
        
        // Fermer le modal si nécessaire
        if (this.modalInstance) {
          this.modalInstance.hide();
        }
      },
      error: (err) => {
        console.error('Erreur lors de l\'ajout de la caméra à la position', err);
        this.error = 'Erreur lors de la mise à jour de la position';
      }
    });
  }
}