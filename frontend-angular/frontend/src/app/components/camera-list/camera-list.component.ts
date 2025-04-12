import { Component, OnInit } from '@angular/core';
import { Camera, CameraService } from '../../services/camera.service';

@Component({
  selector: 'app-camera-list',
  templateUrl: './camera-list.component.html',
  styleUrls: ['./camera-list.component.scss']
})
export class CameraListComponent implements OnInit {
  cameras: Camera[] = [];
  loading = true;
  error = '';

  constructor(private cameraService: CameraService) { }

  ngOnInit(): void {
    this.loadCameras();
  }

  loadCameras(): void {
    this.loading = true;
    this.cameraService.getCameras().subscribe({
      next: (data) => {
        this.cameras = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erreur lors du chargement des caméras';
        console.error(err);
        this.loading = false;
      }
    });
  }

  toggleCameraVisibility(camera: Camera): void {
    const updatedCamera: Camera = {
      ...camera,
      estAfficher: !camera.estAfficher
    };
    
    this.cameraService.updateCamera(updatedCamera).subscribe({
      next: () => {
        // Mettre à jour localement
        camera.estAfficher = !camera.estAfficher;
      },
      error: (err) => {
        console.error('Erreur lors de la mise à jour de la caméra', err);
      }
    });
  }
}