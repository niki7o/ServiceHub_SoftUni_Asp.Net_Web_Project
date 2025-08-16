import { Component } from '@angular/core';

import { Observable } from 'rxjs';
import { CommonModule } from '@angular/common';
import { PostItem } from '../posts/post-item/post-item';
import { Post } from '../../models/post.model';
import { PostService } from '../../core/services/post.service';

@Component({
  selector: 'app-post-board',
  imports: [CommonModule, PostItem],
  templateUrl: './post-board.html',
  styleUrl: './post-board.css'
})
export class PostBoard {
  posts: Post[] = [];
  posts$: Observable<Post[]>;

  constructor(private postsService: PostService) {
    this.posts$ = this.postsService.getPosts();
  }
}