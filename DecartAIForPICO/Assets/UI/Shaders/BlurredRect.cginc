// Based on Fast Rounded Rectangle Shadows by Evan Wallace
// https://madebyevan.com/shaders/fast-rounded-rectangle-shadows/

// A standard gaussian function, used for weighting samples
float gaussian(float x, float sigma)
{
  const float pi = 3.141592653589793;
  return exp(-(x * x) / (2.0 * sigma * sigma)) / (sqrt(2.0 * pi) * sigma);
}

// This approximates the error function, needed for the gaussian integral
float2 erf(float2 x)
{
  float2 s = sign(x), a = abs(x);
  x = 1.0 + (0.278393 + (0.230389 + 0.078108 * (a * a)) * a) * a;
  x *= x;
  return s - s / (x * x);
}

// Return the blurred mask along the x dimension
float roundedBoxShadowX(float x, float y, float sigma, half4 radius, float2 halfSize)
{
  // find the closest radius value for the current pixel
  radius = radius.yzxw;
  radius.xy = (x > 0.0) ? radius.xy : radius.zw;
  radius.x  = (y > 0.0) ? radius.x  : radius.y;

  // roundedBoxShadowX
  float delta = min(halfSize.y - radius.x - abs(y), 0.0);
  float curved = halfSize.x - radius.x + sqrt(max(0.0, radius.x * radius.x - delta * delta));
  float2 integral = 0.5 + 0.5 * erf((x + float2(-curved, curved)) * (sqrt(0.5) / sigma));
  return integral.y - integral.x;
}

// Return the mask for the shadow of a box from lower to upper
float roundedBoxShadow(float2 pos, float2 size, float sigma, half4 radius)
{
  // The signal is only non-zero in a limited range, so don't waste samples
  float low = pos.y - size.y;
  float high = pos.y + size.y;
  float start = clamp(-3.0 * sigma, low, high);
  float end = clamp(3.0 * sigma, low, high);
  
  // Accumulate samples (we can get away with surprisingly few samples)
  float step = (end - start) / 4.0;
  float y = start + step * 0.5;
  float value = 0.0;
  for (int i = 0; i < 4; i++)
  {
    value += roundedBoxShadowX(pos.x, pos.y - y, sigma, radius, size) * gaussian(y, sigma) * step;
    y += step;
  }

  return value;
}