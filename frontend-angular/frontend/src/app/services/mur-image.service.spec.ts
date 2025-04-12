import { TestBed } from '@angular/core/testing';

import { MurImageService } from './mur-image.service';

describe('MurImageService', () => {
  let service: MurImageService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MurImageService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
