# Use the official Golang image
FROM golang:1.20

# Set the working directory
WORKDIR /app

# Copy go.mod and go.sum files
COPY go.mod go.sum ./

# Download dependencies
RUN go mod download

# Copy the source code
COPY . .

# Build the application
RUN go build -o app ./cmd/proxy

# Expose the application port (if applicable)
EXPOSE 1434

# Command to run the application
CMD ["./app"]