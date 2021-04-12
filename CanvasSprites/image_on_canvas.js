// ***** PROTOTYPE ***** //
// function Sprite(imageFile) {
//   this.image = new Image();
//   this.image.src = imageFile;
//   this.x = null;
//   this.y = null;
// }

// Sprite.prototype.isHit = function(mX, mY) {
//   return mX > this.x && mX < this.x + this.image.width &&
//          mY > this.y && mY < this.y + this.image.height;
// }

// Sprite.prototype.draw = function(g){
//   g.drawImage(this.image, this.x, this.y, this.image.width, this.image.height);
// }

// *****  CLASS   ***** //
class Sprite {
  constructor(imageFile) {
    this.image = new Image();
    this.image.src = imageFile;
    this.x = null;
    this.y = null;
  }

  isHit(mX, mY) {
    return mX > this.x && mX < this.x + this.image.width &&
           mY > this.y && mY < this.y + this.image.height;
  }

  draw(g) {
    g.drawImage(this.image, this.x, this.y, this.image.width, this.image.height);
  }
}

// ***** SOLUTION ***** //
r = {
  canvasSelector: '#theCanvas',

  ball: null,

  init: function () {
    r.ball = new Sprite('ball.png');

    // mouse events
    $(r.canvasSelector).on('mousedown', r.mouseDown);

    // complete init when image has been loaded
    $(r.ball.image).one('load', function () {
      // initial location of image on canvas
      r.ball.x = ($(r.canvasSelector)[0].width - this.width) / 2;
      r.ball.y = ($(r.canvasSelector)[0].height - this.height) / 2;

      r.render();
    });
  },

  mouseDown: function (e) {
    if (r.ball.isHit(r.getX(e), r.getY(e))) {
      $(r.canvasSelector).on('mousemove', r.mouseMove);

      $(r.canvasSelector).on('mouseup', function () {
        $(r.canvasSelector).off('mousemove', r.mouseMove);
      });
    }
  },

  mouseMove: function (e) {
    r.ball.x = r.getX(e) - r.ball.image.width / 2;
    r.ball.y = r.getY(e) - r.ball.image.height / 2;

    r.render();
  },

  getX: function (e) {
    return e.pageX - $(r.canvasSelector).offset().left;
  },

  getY: function (e) {
    return e.pageY - $(r.canvasSelector).offset().top;
  },

  render: function () {
    var canvas = $(r.canvasSelector)[0];

    // 'g' for 'Graphics' (as in Java paint-method)
    var g = canvas.getContext('2d');

    // render (i.e. clear) background
    g.fillStyle = '#FFB'; // pale yellow
    g.fillRect(0, 0, canvas.width, canvas.height);

    // render image
    r.ball.draw(g);

    // render border
    g.strokeStyle = '#000'; // black
    g.strokeRect(0, 0, canvas.width, canvas.height);
  }
};

$(r.init);