#version 110

uniform sampler2D   data;
uniform vec2        dataSize;
uniform sampler2D   palette;

vec4 texelGet(sampler2D tex, vec2 size, vec2 coord) {
    return texture2D(tex, vec2(coord.x / size.x,
                               coord.y / size.y));
}

void main() {
    vec2 datPos = vec2(floor(gl_TexCoord[0].x * dataSize.x),
                       floor(gl_TexCoord[0].y * dataSize.y));

    // r - color
    vec4 dat = texelGet(data, dataSize - 1.0, datPos);
    float col = floor(dat.r * 256.0);

    gl_FragColor = texelGet(palette, vec2(256.0, 1.0), vec2(col, 0.0));
}
